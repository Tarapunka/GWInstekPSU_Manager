using GWInstekPSUManager.Core.Interfaces.ChannelInterfaces;
using GWInstekPSUManager.Core.Models;
using System.Diagnostics;
using System.Text;

namespace GWInstekPSUManager.Infrastructure.Services.ChannelServices.LoggerServices;

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