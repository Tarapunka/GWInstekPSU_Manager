using GWInstekPSUManager.Core.Interfaces.ChannelInterfaces;
using GWInstekPSUManager.Core.Models;

namespace GWInstekPSUManager.Infrastructure.Services.ChannelServices;

public class ChannelContext : IDisposable
{
    public IPowerSupplyChannel Channel { get; }
    public CancellationTokenSource Cts { get; }
    public SingleChannelLogger Logger { get; }

    public ChannelContext(IPowerSupplyChannel channel,
                        CancellationTokenSource cts,
                        SingleChannelLogger logger)
    {
        Channel = channel ?? throw new ArgumentNullException(nameof(channel));
        Cts = cts ?? throw new ArgumentNullException(nameof(cts));
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void Dispose()
    {
        Cts?.Cancel();
        Logger?.Dispose();
    }
}