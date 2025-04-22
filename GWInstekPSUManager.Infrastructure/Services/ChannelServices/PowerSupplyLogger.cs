using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Diagnostics;
using GWInstekPSUManager.Core.Interfaces.ChannelInterfaces;

namespace GWInstekPSUManager.Infrastructure.Services.ChannelServices;

public class PowerSupplyLogger : IDisposable
{
    private readonly IPowerSupplyChannel _channel;
    private StreamWriter _writer;
    private bool _disposed;
    private readonly object _lock = new();
    private string _currentLogFilePath;
    private bool _isLogging;
    private DateTime? _startTime;
    private readonly string _logsDirectory;

    public PowerSupplyLogger(IPowerSupplyChannel channel)
    {
        _channel = channel ?? throw new ArgumentNullException(nameof(channel));
        _logsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ChannelLogs");
        Directory.CreateDirectory(_logsDirectory); // Создаем папку при инициализации

        _channel.PropertyChanged += OnPropertyChanged;

        // Начинаем запись сразу при создании логгера, если канал включен
        if (_channel.IsEnabled)
        {
            StartNewLog();
        }
    }

    public void StartNewLog()
    {
        lock (_lock)
        {
            if (_isLogging)
            {
                StopLog(); // Останавливаем текущую запись перед началом новой
            }

            // Генерируем уникальное имя файла
            _currentLogFilePath = Path.Combine(
                _logsDirectory,
                $"Channel_{_channel.ChannelNumber}_{DateTime.Now:yyyyMMdd_HHmmssfff}.csv");

            try
            {
                _writer = new StreamWriter(_currentLogFilePath);
                WriteHeader();
                _startTime = DateTime.Now;
                _isLogging = true;

                // Записываем текущее состояние сразу после создания файла
                LogCurrentState();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to start new log: {ex.Message}");
                _writer?.Dispose();
                _writer = null;
                _isLogging = false;
            }
        }
    }

    public void StopLog()
    {
        lock (_lock)
        {
            if (!_isLogging) return;

            _writer?.Flush();
            _writer?.Dispose();
            _writer = null;

            // Сбрасываем время начала записи
            _startTime = null;

            _isLogging = false;
        }
    }

    private void WriteHeader()
    {
        var header = "Timestamp,ChannelNumber,Voltage(V),Current(A),Power(W),Capacity(Ah)," +
                    "CurrentLimit(A),VoltageLimit(V),Mode,StartTime,ElapsedTime,IsEnabled,IsCalibrated";
        _writer.WriteLine(header);
        _writer.Flush();
    }

    private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IPowerSupplyChannel.IsEnabled))
        {
            if (_channel.IsEnabled)
            {
                StartNewLog();
            }
            else
            {
                StopLog();
            }
        }

        if (_isLogging && _writer != null)
        {
            LogCurrentState();
        }
    }

    public void LogCurrentState()
    {
        lock (_lock)
        {
            if (!_isLogging || _writer == null || !_startTime.HasValue) return;

            try
            {
                // Рассчитываем прошедшее время с момента включения канала
                var elapsedTime = DateTime.Now - _startTime.Value;

                var timestamp = DateTime.Now.ToString("o");
                var line = $"{timestamp}," +
                          $"{_channel.DeviceName}," +
                          $"{_channel.ChannelNumber}," +
                          $"{_channel.Voltage.ToString(CultureInfo.InvariantCulture)}," +
                          $"{_channel.Current.ToString(CultureInfo.InvariantCulture)}," +
                          $"{_channel.Power.ToString(CultureInfo.InvariantCulture)}," +
                          $"{_channel.Capacity.ToString(CultureInfo.InvariantCulture)}," +
                          $"{_channel.CurrentLimit.ToString(CultureInfo.InvariantCulture)}," +
                          $"{_channel.VoltageLimit.ToString(CultureInfo.InvariantCulture)}," +
                          $"{EscapeCsv(_channel.Mode)}," +
                          $"{_startTime.Value.ToString("o")}," + // Время начала записи
                          $"{elapsedTime.TotalSeconds}," +      // Прошедшее время в секундах
                          $"{_channel.IsEnabled}," +
                          $"{_channel.IsCalibrated}";

                _writer.WriteLine(line);
                _writer.Flush();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error logging data: {ex.Message}");
            }
        }
    }

    private static string EscapeCsv(string input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;
        return input.Contains(",") ? $"\"{input}\"" : input;
    }

    public void Dispose()
    {
        lock (_lock)
        {
            if (_disposed) return;

            _channel.PropertyChanged -= OnPropertyChanged;
            StopLog();
            _disposed = true;
        }
    }
}