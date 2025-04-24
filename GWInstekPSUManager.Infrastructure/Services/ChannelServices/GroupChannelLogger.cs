using GWInstekPSUManager.Core.Interfaces.ChannelInterfaces;
using GWInstekPSUManager.Core.Interfaces.DeviceInterfaces;
using GWInstekPSUManager.Core.Models;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace GWInstekPSUManager.Infrastructure.Services.ChannelServices;

public class GroupLoggerService : IDisposable
{
    private readonly ConcurrentDictionary<int, IPowerSupplyChannel> _channels = new();
    private StreamWriter _writer;
    private readonly object _lock = new();
    private DateTime _startTime;

    public void AddChannel(int channelNumber, IPowerSupplyChannel channel)
    {
        _channels.TryAdd(channelNumber, channel);
        if (_channels.Count == 1)
        {
            StartNewLog();
        }
    }

    public void RemoveChannel(int channelNumber)
    {
        _channels.TryRemove(channelNumber, out _);
        if (_channels.IsEmpty)
        {
            CloseLog();
        }
    }

    private void StartNewLog()
    {
        lock (_lock)
        {
            CloseLog();

            var logFile = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Logs",
                $"GroupLog_{DateTime.Now:yyyyMMdd_HHmmss}.csv");

            Directory.CreateDirectory(Path.GetDirectoryName(logFile));
            _writer = new StreamWriter(logFile, false, Encoding.UTF8);
            _startTime = DateTime.Now;

            var header = "Timestamp;" +
                string.Join(";", _channels.OrderBy(c => c.Key)
                    .Select(c => $"Ch{c.Key}_V;Ch{c.Key}_I;Ch{c.Key}_P;Ch{c.Key}_C")) +
                ";TotalV;TotalI;TotalP;TotalC";

            _writer.WriteLine(header);
            _writer.Flush();
        }
    }

    public void LogGroupState()
    {
        lock (_lock)
        {
            if (_writer == null || _channels.IsEmpty) return;

            try
            {
                var currentTime = DateTime.Now;
                var sb = new StringBuilder($"{currentTime:o};");

                double totalV = 0, totalI = 0, totalP = 0, totalC = 0;

                foreach (var ch in _channels.OrderBy(c => c.Key))
                {
                    sb.Append($"{ch.Value.Voltage.ToString(CultureInfo.InvariantCulture)};");
                    sb.Append($"{ch.Value.Current.ToString(CultureInfo.InvariantCulture)};");
                    sb.Append($"{ch.Value.Power.ToString(CultureInfo.InvariantCulture)};");
                    sb.Append($"{ch.Value.Capacity.ToString(CultureInfo.InvariantCulture)};");

                    totalV += ch.Value.Voltage;
                    totalI += ch.Value.Current;
                    totalP += ch.Value.Power;
                    totalC += ch.Value.Capacity;
                }

                sb.Append($"{totalV.ToString(CultureInfo.InvariantCulture)};");
                sb.Append($"{totalI.ToString(CultureInfo.InvariantCulture)};");
                sb.Append($"{totalP.ToString(CultureInfo.InvariantCulture)};");
                sb.Append($"{totalC.ToString(CultureInfo.InvariantCulture)}");

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
        CloseLog();
    }
}
