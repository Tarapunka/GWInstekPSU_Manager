using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Diagnostics;
using GWInstekPSUManager.Core.Interfaces.ChannelInterfaces;

namespace GWInstekPSUManager.Infrastructure.Services.ChannelServices
{
    public class PowerSupplyLogger : IDisposable
    {
        private readonly IPowerSupplyChannel _channel;
        private readonly Func<int, IPowerSupplyChannel> _getChannelFunc;
        private StreamWriter _writer;
        private bool _disposed;
        private readonly object _lock = new();
        private string _currentLogFilePath;
        private DateTime? _testStartTime;
        private readonly string _logsDirectory;

        public PowerSupplyLogger(IPowerSupplyChannel channel, Func<int, IPowerSupplyChannel> getChannelFunc = null)
        {
            _channel = channel ?? throw new ArgumentNullException(nameof(channel));
            _getChannelFunc = getChannelFunc;

            _logsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ChannelLogs");
            Directory.CreateDirectory(_logsDirectory);

            _channel.PropertyChanged += OnPropertyChanged;

            // Сразу создаем лог если канал включен
            if (_channel.IsEnabled)
            {
                CreateNewLogFile();
            }
        }

        private void CreateNewLogFile()
        {
            lock (_lock)
            {
                // Закрываем предыдущий лог если был
                if (_writer != null)
                {
                    _writer.Flush();
                    _writer.Dispose();
                    _writer = null;
                }

                // Создаем новый файл с уникальным именем
                _currentLogFilePath = Path.Combine(
                    _logsDirectory,
                    $"{_channel.DeviceName}_Channel_{_channel.ChannelNumber}_{DateTime.Now:yyyyMMdd_HHmmssfff}.csv");

                _writer = new StreamWriter(_currentLogFilePath, false, Encoding.UTF8);
                _testStartTime = DateTime.Now;

                WriteHeader();
                LogCurrentState(); // Записываем начальное состояние
            }
        }

        private IPowerSupplyChannel[] GetGroupChannels()
        {
            if (!(_channel.IsSeriesOn || _channel.IsParallelOn) || _getChannelFunc == null)
                return Array.Empty<IPowerSupplyChannel>();

            try
            {
                return Enumerable.Range(1, 4) // Предполагаем 4 канала
                    .Select(chNum => _getChannelFunc(chNum))
                    .Where(ch => ch != null &&
                               ch.IsSeriesOn == _channel.IsSeriesOn &&
                               ch.IsParallelOn == _channel.IsParallelOn &&
                               ch.ChannelNumber != _channel.ChannelNumber) // Исключаем текущий канал
                    .OrderBy(ch => ch.ChannelNumber)
                    .ToArray();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting group channels: {ex.Message}");
                return Array.Empty<IPowerSupplyChannel>();
            }
        }

        private void WriteHeader()
        {
            var header = new StringBuilder("DeviceName;ChannelNumber;");

            if (_channel.IsSeriesOn || _channel.IsParallelOn)
            {
                var groupChannels = GetGroupChannels();

                // Напряжение
                header.Append($"Channel{_channel.ChannelNumber}Voltage;");
                foreach (var ch in groupChannels)
                {
                    header.Append($"Channel{ch.ChannelNumber}Voltage;");
                }
                header.Append("TotalVoltage;");

                // Ток
                header.Append($"Channel{_channel.ChannelNumber}Current;");
                foreach (var ch in groupChannels)
                {
                    header.Append($"Channel{ch.ChannelNumber}Current;");
                }
                header.Append("TotalCurrent;");

                // Мощность
                header.Append($"Channel{_channel.ChannelNumber}Power;");
                foreach (var ch in groupChannels)
                {
                    header.Append($"Channel{ch.ChannelNumber}Power;");
                }
                header.Append("TotalPower;");

                // Ёмкость
                header.Append($"Channel{_channel.ChannelNumber}Capacity;");
                foreach (var ch in groupChannels)
                {
                    header.Append($"Channel{ch.ChannelNumber}Capacity;");
                }
                header.Append("TotalCapacity;");
            }
            else
            {
                header.Append("Voltage;Current;Power;Capacity;");
            }

            header.Append("CurrentTime;TestTimeSec");
            _writer.WriteLine(header.ToString());
            _writer.Flush();
        }

        public void LogCurrentState()
        {
            lock (_lock)
            {
                if (_writer == null || !_channel.IsEnabled) return;

                try
                {
                    var currentTime = DateTime.Now;
                    var testTime = currentTime - _testStartTime.Value;
                    var line = new StringBuilder();

                    line.Append($"{EscapeCsv(_channel.DeviceName)};{_channel.ChannelNumber};");

                    if (_channel.IsSeriesOn || _channel.IsParallelOn)
                    {
                        var groupChannels = GetGroupChannels();
                        double totalVoltage = _channel.Voltage;
                        double totalCurrent = _channel.Current;
                        double totalCapacity = _channel.Capacity;

                        // Текущий канал
                        line.Append($"{_channel.Voltage.ToString(CultureInfo.InvariantCulture)};");

                        // Остальные каналы в группе
                        foreach (var ch in groupChannels)
                        {
                            line.Append($"{ch.Voltage.ToString(CultureInfo.InvariantCulture)};");
                            totalVoltage += ch.Voltage;
                        }
                        line.Append($"{totalVoltage.ToString(CultureInfo.InvariantCulture)};");

                        // Ток - аналогично
                        line.Append($"{_channel.Current.ToString(CultureInfo.InvariantCulture)};");
                        foreach (var ch in groupChannels)
                        {
                            line.Append($"{ch.Current.ToString(CultureInfo.InvariantCulture)};");
                            totalCurrent += ch.Current;
                        }
                        line.Append($"{totalCurrent.ToString(CultureInfo.InvariantCulture)};");

                        // Мощность
                        line.Append($"{(_channel.Voltage * _channel.Current).ToString(CultureInfo.InvariantCulture)};");
                        foreach (var ch in groupChannels)
                        {
                            line.Append($"{(ch.Voltage * ch.Current).ToString(CultureInfo.InvariantCulture)};");
                        }
                        line.Append($"{(totalVoltage * totalCurrent).ToString(CultureInfo.InvariantCulture)};");

                        // Ёмкость
                        line.Append($"{_channel.Capacity.ToString(CultureInfo.InvariantCulture)};");
                        foreach (var ch in groupChannels)
                        {
                            line.Append($"{ch.Capacity.ToString(CultureInfo.InvariantCulture)};");
                            totalCapacity += ch.Capacity;
                        }
                        line.Append($"{totalCapacity.ToString(CultureInfo.InvariantCulture)};");
                    }
                    else
                    {
                        // Одиночный режим
                        line.Append($"{_channel.Voltage.ToString(CultureInfo.InvariantCulture)};");
                        line.Append($"{_channel.Current.ToString(CultureInfo.InvariantCulture)};");
                        line.Append($"{(_channel.Voltage * _channel.Current).ToString(CultureInfo.InvariantCulture)};");
                        line.Append($"{_channel.Capacity.ToString(CultureInfo.InvariantCulture)};");
                    }

                    line.Append($"{currentTime:o};{testTime.TotalSeconds.ToString(CultureInfo.InvariantCulture)}");
                    _writer.WriteLine(line.ToString());
                    _writer.Flush();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error logging data: {ex.Message}");
                }
            }
        }
        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IPowerSupplyChannel.IsEnabled))
            {
                if (_channel.IsEnabled)
                {
                    // Канал включен - создаем новый файл
                    CreateNewLogFile();
                }
                else
                {
                    // Канал выключен - закрываем файл
                    lock (_lock)
                    {
                        _writer?.Flush();
                        _writer?.Dispose();
                        _writer = null;
                    }
                }
            }

            // Логируем изменения состояния
            if (_channel.IsEnabled && _writer != null)
            {
                LogCurrentState();
            }
        }

        private static string EscapeCsv(string input) =>
            string.IsNullOrEmpty(input) ? string.Empty :
            input.Contains(";") ? $"\"{input}\"" : input;

        public void Dispose()
        {
            lock (_lock)
            {
                if (_disposed) return;
                _disposed = true;
                _channel.PropertyChanged -= OnPropertyChanged;
                _writer?.Flush();
                _writer?.Dispose();
            }
        }
    }
}