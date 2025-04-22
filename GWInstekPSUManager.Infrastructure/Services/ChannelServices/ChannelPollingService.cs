using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GWInstekPSUManager.Core.Events;
using GWInstekPSUManager.Core.Interfaces.ChannelInterfaces;
using GWInstekPSUManager.Core.Interfaces.DeviceInterfaces;
using GWInstekPSUManager.Core.Models;

namespace GWInstekPSUManager.Infrastructure.Services.ChannelServices
{
    public class ChannelPollingService : IChannelPollingService, IDisposable
    {
        private readonly IDeviceService _deviceService;
        private readonly ConcurrentDictionary<int, ChannelPollingContext> _activePolls = new();
        private readonly object _syncRoot = new();
        private readonly PowerSupplyLoggerFactory _loggerFactory;

        public event EventHandler<ChannelMeasurementEventArgs> MeasurementReceived;
        public event Action<int> ChannelLimitExceeded;

        public ChannelPollingService(IDeviceService deviceService)
        {
            _deviceService = deviceService ?? throw new ArgumentNullException(nameof(deviceService));
            _loggerFactory = new PowerSupplyLoggerFactory();
        }

        public async Task StartPollingAsync(int channelNumber, IPowerSupplyChannel channel)
        {
            await StopPollingAsync(channelNumber).ConfigureAwait(false);

            var context = new ChannelPollingContext
            {
                Channel = channel,
                Cts = new CancellationTokenSource(),
                Logger = _loggerFactory.GetOrCreateLogger(channel),
                CapacityCalculator = new ChannelCapacityCalculator()
            };

            _activePolls[channelNumber] = context;

            _ = Task.Run(() => PollChannelAsync(channelNumber, context), context.Cts.Token);
        }

        private async Task PollChannelAsync(int channelNumber, ChannelPollingContext context)
        {
            try
            {
                var token = context.Cts.Token;
                var channel = context.Channel;

                while (!token.IsCancellationRequested)
                {
                    var sw = Stopwatch.StartNew();
                    var measurementTime = DateTime.Now;

                    // Для связанных каналов - групповой опрос
                    if (channel.IsSeriesOn || channel.IsParallelOn)
                    {
                        await PollLinkedChannelsGroup(channelNumber, measurementTime, token);
                    }
                    else
                    {
                        await PollSingleChannel(channelNumber, context, sw, measurementTime, token);
                    }

                    await Task.Delay(GetPollingInterval(channel), token);
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                Debug.WriteLine($"Polling error for channel {channelNumber}: {ex.Message}");
            }
            finally
            {
                _activePolls.TryRemove(channelNumber, out _);
                context.Cts.Dispose();
            }
        }

        private async Task PollLinkedChannelsGroup(int masterChannelNumber, DateTime measurementTime, CancellationToken token)
        {
            var masterContext = _activePolls[masterChannelNumber];
            var masterChannel = masterContext.Channel;

            // Находим все каналы в той же группе (Series/Parallel)
            var groupChannels = _activePolls
                .Where(x => x.Value.Channel.IsSeriesOn == masterChannel.IsSeriesOn &&
                           x.Value.Channel.IsParallelOn == masterChannel.IsParallelOn)
                .ToList();

            // Сначала собираем все измерения
            var measurements = new ConcurrentDictionary<int, MeasureResponse>();
            await Task.WhenAll(groupChannels.Select(async channel =>
            {
                if (token.IsCancellationRequested)
                    return;

                var response = await _deviceService.GetMeasureChannelAsync(channel.Key);
                if (!token.IsCancellationRequested)
                {
                    measurements[channel.Key] = response;
                }
            }));

            // Затем обрабатываем результаты
            foreach (var channel in groupChannels)
            {
                if (token.IsCancellationRequested)
                    break;

                if (measurements.TryGetValue(channel.Key, out var channelMeasurements))
                {
                    ProcessChannelMeasurements(
                        channel.Key,
                        channel.Value.Channel,
                        channelMeasurements,
                        channel.Value.Logger,
                        channel.Value.CapacityCalculator,
                        measurementTime);
                }
            }
        }

        private async Task PollSingleChannel(int channelNumber, ChannelPollingContext context,
                                          Stopwatch sw, DateTime measurementTime, CancellationToken token)
        {
            var measurements = await _deviceService.GetMeasureChannelAsync(channelNumber);
            if (!token.IsCancellationRequested)
            {
                sw.Stop();
                ProcessChannelMeasurements(
                    channelNumber,
                    context.Channel,
                    measurements,
                    context.Logger,
                    context.CapacityCalculator,
                    measurementTime,
                    sw.Elapsed);
            }
        }

        private void ProcessChannelMeasurements(
            int channelNumber,
            IPowerSupplyChannel channel,
            MeasureResponse measurements,
            PowerSupplyLogger logger,
            ChannelCapacityCalculator capacityCalculator,
            DateTime measurementTime,
            TimeSpan? elapsedTime = null)
        {
            lock (_syncRoot)
            {
                // Обновляем состояние канала
                channel.Voltage = measurements.Voltage;
                channel.Current = measurements.Current;
                channel.Power = measurements.Voltage * measurements.Current;

                // Обновляем емкость с использованием точного расчета
                if (channel.IsEnabled)
                {
                    channel.Capacity = capacityCalculator.CalculateCapacity(measurements.Current, measurementTime);
                }

                // Проверка лимитов
                if (IsLimitExceeded(channel, measurements))
                {
                    ChannelLimitExceeded?.Invoke(channelNumber);
                    return;
                }

                // Логирование
                logger.LogCurrentState();

                // Уведомление UI
                MeasurementReceived?.Invoke(this, new ChannelMeasurementEventArgs(
                    channelNumber,
                    measurements,
                    elapsedTime ?? TimeSpan.Zero,
                    channel.Capacity));
            }
        }

        private int GetPollingInterval(IPowerSupplyChannel channel)
        {
            // Можно настроить разные интервалы для разных режимов
            return 500; // стандартный интервал
        }

        public async Task StopPollingAsync(int channelNumber)
        {
            if (_activePolls.TryRemove(channelNumber, out var context))
            {
                context.Cts.Cancel();
                try { await Task.Delay(100); } catch { }
                context.Cts.Dispose();
            }
        }

        public void ResetCapacityCounter(int channelNumber)
        {
            lock (_syncRoot)
            {
                if (_activePolls.TryGetValue(channelNumber, out var context))
                {
                    context.Channel.Capacity = 0;
                    context.CapacityCalculator.Reset();
                }
            }
        }

        public void Dispose()
        {
            foreach (var channel in _activePolls.Keys.ToArray())
            {
                StopPollingAsync(channel).Wait();
            }
            _loggerFactory.Dispose();
        }

        private bool IsLimitExceeded(IPowerSupplyChannel channel, MeasureResponse measurements)
        {
            return (channel.VoltageLimit > 0 && measurements.Voltage >= channel.VoltageLimit) ||
                   (channel.CurrentLimit > 0 && measurements.Current >= channel.CurrentLimit);
        }

        private class ChannelPollingContext
        {
            public IPowerSupplyChannel Channel { get; set; }
            public CancellationTokenSource Cts { get; set; }
            public PowerSupplyLogger Logger { get; set; }
            public ChannelCapacityCalculator CapacityCalculator { get; set; }
        }
    }

    public class PowerSupplyLoggerFactory : IDisposable
    {
        private readonly ConcurrentDictionary<int, PowerSupplyLogger> _loggers = new();

        public PowerSupplyLogger GetOrCreateLogger(IPowerSupplyChannel channel)
        {
            return _loggers.GetOrAdd(channel.ChannelNumber, _ => new PowerSupplyLogger(channel));
        }

        public void Dispose()
        {
            foreach (var logger in _loggers.Values)
            {
                logger.Dispose();
            }
            _loggers.Clear();
        }
    }
}