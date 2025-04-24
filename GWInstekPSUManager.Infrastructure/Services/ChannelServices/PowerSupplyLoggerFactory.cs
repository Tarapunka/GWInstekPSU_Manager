using GWInstekPSUManager.Core.Interfaces.ChannelInterfaces;
using GWInstekPSUManager.Infrastructure.Services.ChannelServices;
using System.Collections.Concurrent;

public sealed class PowerSupplyLoggerFactory : IDisposable
{
    private readonly ConcurrentDictionary<int, PowerSupplyLogger> _loggers = new();
    private readonly Func<int, IPowerSupplyChannel> _getChannelFunc;
    private bool _disposed;

    public PowerSupplyLoggerFactory(Func<int, IPowerSupplyChannel> getChannelFunc)
    {
        _getChannelFunc = getChannelFunc ?? throw new ArgumentNullException(nameof(getChannelFunc));
    }

    public PowerSupplyLogger GetOrCreateLogger(IPowerSupplyChannel channel)
    {
        if (_disposed) throw new ObjectDisposedException(GetType().Name);

        return _loggers.GetOrAdd(channel.ChannelNumber, _ =>
            new PowerSupplyLogger(channel, _getChannelFunc));
    }

    public void RemoveLogger(int channelNumber)
    {
        if (_disposed) throw new ObjectDisposedException(GetType().Name);

        if (_loggers.TryRemove(channelNumber, out var logger))
        {
            logger.Dispose();
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        foreach (var logger in _loggers.Values)
        {
            logger.Dispose();
        }
        _loggers.Clear();
    }
}