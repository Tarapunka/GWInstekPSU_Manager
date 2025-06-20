﻿using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using GWInstekPSUManager.Core.Events;
using GWInstekPSUManager.Core.Interfaces.ChannelInterfaces;
using GWInstekPSUManager.Core.Interfaces.DeviceInterfaces;
using GWInstekPSUManager.Core.Models;
using GWInstekPSUManager.Infrastructure.Services.ChannelServices.LoggerServices;

namespace GWInstekPSUManager.Infrastructure.Services.ChannelServices
{
    public class ChannelPollingService : IChannelPollingService, IDisposable
    {
        private readonly IDeviceService _deviceService;
        private readonly ConcurrentDictionary<int, ChannelContext> _activeChannels = new();
        private readonly GroupChannelLogger _groupLogger = new();
        private bool _disposed;

        public event EventHandler<ChannelMeasurementEventArgs> MeasurementReceived;
        public event Action<int> ChannelLimitExceeded;

        public ChannelPollingService(IDeviceService deviceService)
        {
            _deviceService = deviceService ?? throw new ArgumentNullException(nameof(deviceService));
        }

        public async Task StartPollingAsync(int channelNumber, IPowerSupplyChannel channel)
        {
            if (_disposed) throw new ObjectDisposedException(GetType().Name);

            await StopPollingAsync(channelNumber).ConfigureAwait(false);

            // Даём время на корректное завершение
            await Task.Delay(100);

            // Только если канал был выключен – сбрасываем счётчик ёмкости
            if (!channel.IsEnabled && channel.CapacityCalculator != null)
            {
                channel.CapacityCalculator.Reset();
            }

            var context = new ChannelContext(
                channel,
                new CancellationTokenSource(),
                new SingleChannelLogger(channelNumber, channel.DeviceName));

            if (!_activeChannels.TryAdd(channelNumber, context))
            {
                context.Dispose();
                throw new InvalidOperationException($"Polling for channel {channelNumber} is already running");
            }

            if (channel.IsSeriesOn || channel.IsParallelOn)
            {
                _groupLogger.AddChannel(channelNumber, channel);
            }

            _ = Task.Run(() => PollChannelAsync(channelNumber, context), context.Cts.Token);
        }

        private async Task PollChannelAsync(int channelNumber, ChannelContext context)
        {
            try
            {
                while (!context.Cts.IsCancellationRequested)
                {
                    var sw = Stopwatch.StartNew();
                    var measurement = await _deviceService.GetMeasureChannelAsync(channelNumber);
                    sw.Stop();

                    ProcessMeasurement(channelNumber, context, measurement, sw.Elapsed);
                    await Task.Delay(1000, context.Cts.Token);
                }
            }
            catch (OperationCanceledException)
            {
                // Нормальное завершение
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Polling error for channel {channelNumber}: {ex.Message}");
                HandlePollingTaskFault(channelNumber);
            }
        }

        private void ProcessMeasurement(int channelNumber, ChannelContext context,
        MeasureResponse measurement, TimeSpan elapsed)
        {
            context.Channel.Voltage = measurement.Voltage;
            context.Channel.Current = measurement.Current;
            context.Channel.Power = measurement.Power;

            if (/*context.Channel.IsEnabled &&*/ context.Channel.CapacityCalculator != null)
            {
                context.Channel.Capacity = context.Channel.CapacityCalculator.CalculateCapacity(
                    measurement.Current, DateTime.Now);
            }

            bool limitsExceeded;

            lock (context.Channel)
            {
                limitsExceeded = CheckLimits(context.Channel, measurement);

                // Логирование
                context.Logger.LogMeasurement(context.Channel, measurement, elapsed);

                if (context.Channel.IsSeriesOn || context.Channel.IsParallelOn)
                {
                    _groupLogger.LogMeasurement(context.Channel, measurement, elapsed);
                }

            }

            // Проверка лимитов
            if (CheckLimits(context.Channel, measurement))
            {
                ChannelLimitExceeded?.Invoke(channelNumber);
            }

            // Отправка события
            MeasurementReceived?.Invoke(this, new ChannelMeasurementEventArgs(
                channelNumber,
                measurement,
                elapsed,
                context.Channel.Capacity));
        }

        private bool CheckLimits(IPowerSupplyChannel channel, MeasureResponse measurements)
        {
            if (channel.IsSeriesOn)
                return channel.GroupVoltageLimit > 0 && channel.GroupActualVoltage >= channel.GroupVoltageLimit;

            if (channel.IsParallelOn)
                return channel.GroupCurrentLimit > 0 && channel.GroupActualCurrent >= channel.GroupCurrentLimit;

            return (measurements.Voltage <= channel.VoltageLimit) ||
                   (measurements.Current <= channel.CurrentLimit);
        }

        public async Task StopPollingAsync(int channelNumber)
        {
            if (_disposed) throw new ObjectDisposedException(GetType().Name);
            if (_activeChannels.TryRemove(channelNumber, out var context))
            {
                try
                {
                    context.Cts.Cancel();
                    await Task.Delay(100, context.Cts.Token).ConfigureAwait(false);
                }
                catch (OperationCanceledException) { }
                finally
                {
                    context.Dispose();
                    _groupLogger.RemoveChannel(channelNumber);
                }
            }
        }

        public void ResetCapacityCounter(int channelNumber)
        {
            if (_disposed) throw new ObjectDisposedException(GetType().Name);
            if (_activeChannels.TryGetValue(channelNumber, out var context))
            {
                lock (context.Channel)
                {
                    context.Channel.Capacity = 0;
                    context.Channel.CapacityCalculator?.Reset();
                }
            }
        }

        private void HandlePollingTaskFault(int channelNumber)
        {
            if (_activeChannels.TryRemove(channelNumber, out var context))
            {
                context.Dispose();
                _groupLogger.RemoveChannel(channelNumber);
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            foreach (var channelNumber in _activeChannels.Keys.ToArray())
            {
                StopPollingAsync(channelNumber).Wait();
            }

            _groupLogger.Dispose();
        }
    }


    public class SingleChannelLogger : IMeasurementLogger
    {
        private StreamWriter _writer;
        private readonly string _logFilePath;
        private DateTime _startTime;

        public SingleChannelLogger(int channelNumber, string deviceName)
        {
            var logsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ChannelLogs");
            Directory.CreateDirectory(logsDir);
            _logFilePath = Path.Combine(logsDir, $"{deviceName}_Ch{channelNumber}_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
            StartNewLog();
        }

        public void StartNewLog()
        {
            _writer?.Dispose();
            _writer = new StreamWriter(_logFilePath, false, Encoding.UTF8);
            _writer.WriteLine("Timestamp;Voltage;Current;Power;Capacity;TestTime");
            _startTime = DateTime.Now;
        }

        public void LogMeasurement(IPowerSupplyChannel channel, MeasureResponse measurement, TimeSpan elapsed)
        {
            try
            {
                var testTime = DateTime.Now - _startTime;
                var line = $"{DateTime.Now:O};{measurement.Voltage};{measurement.Current};" +
                          $"{measurement.Voltage * measurement.Current};{channel.Capacity};" +
                          // Форматирование времени как 00:00:00
                          $"{(int)testTime.TotalHours:D2}:{testTime.Minutes:D2}:{testTime.Seconds:D2}";

                _writer.WriteLine(line);
                _writer.Flush();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error logging measurement: {ex.Message}");
            }
        }
        public void Dispose()
        {
            _writer?.Flush();
            _writer?.Dispose();
        }
    }
}