using GWInstekPSUManager.Core.Interfaces.ChannelInterfaces;
using GWInstekPSUManager.Core.Models;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;

namespace GWInstekPSUManager.Infrastructure.Services.ChannelServices.LoggerServices;

public class GroupChannelLogger : IMeasurementLogger
{
    private readonly object _lock = new object();
    private readonly ConcurrentDictionary<int, IPowerSupplyChannel> _channels = new();
    private StreamWriter _writer;
    private readonly string _logFilePath;
    private DateTime _startTime;

    public GroupChannelLogger()
    {
        var logsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ChannelLogsGroup");
        Directory.CreateDirectory(logsDir);
        _logFilePath = Path.Combine(logsDir, $"GroupLog_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        StartNewLog();
    }

    public void AddChannel(int channelNumber, IPowerSupplyChannel channel)
    {
        _channels.TryAdd(channelNumber, channel);
    }

    public void StartNewLog()
    {
        _writer?.Dispose();
        _writer = new StreamWriter(_logFilePath, false, Encoding.UTF8);

        var header = "Timestamp;" + string.Join(";",
            _channels.OrderBy(c => c.Key).Select(c => $"Ch{c.Key}_V;Ch{c.Key}_I;Ch{c.Key}_P;Ch{c.Key}_C")) +
             ";TotalV;TotalI;TotalP;TotalC;TestTime";

        _writer.WriteLine(header);
        _startTime = DateTime.Now;
    }

    public void RemoveChannel(int channelNumber)
    {
        _channels.TryRemove(channelNumber, out _);
        if (_channels.IsEmpty)
        {
            CloseLog();
        }
    }


    public void LogMeasurement(IPowerSupplyChannel channel, MeasureResponse measurement, TimeSpan elapsed)
    {
        lock (_lock)
        {
            if (_writer == null || _channels.IsEmpty) return;

            try
            {
                var currentTime = DateTime.Now;
                var testTime = currentTime - _startTime;
                var sb = new StringBuilder($"{currentTime:O};");

                double totalV = 0, totalI = 0, totalP = 0, totalC = 0;

                foreach (var ch in _channels.OrderBy(c => c.Key))
                {
                    sb.Append($"{ch.Value.Voltage};{ch.Value.Current};{ch.Value.Power};{ch.Value.Capacity};");
                    totalV += ch.Value.Voltage;
                    totalI += ch.Value.Current;
                    totalP += ch.Value.Power;
                    totalC += ch.Value.Capacity;
                }

                // Форматирование времени как 00:00:00
                sb.Append($"{totalV};{totalI};{totalP};{totalC};");
                sb.Append($"{(int)testTime.TotalHours:D2}:{testTime.Minutes:D2}:{testTime.Seconds:D2}");

                _writer.WriteLine(sb.ToString());
                _writer.Flush();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error logging group state: {ex.Message}");
            }
        }
    }

    private void CloseLog()
    {
        _writer?.Flush();
        _writer?.Dispose();
        _writer = null;
    }

    public void Dispose()
    {
        _writer?.Flush();
        _writer?.Dispose();
    }
}
