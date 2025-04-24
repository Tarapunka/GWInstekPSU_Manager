using GWInstekPSUManager.Core.Interfaces.ChannelInterfaces;

namespace GWInstekPSUManager.Infrastructure.Services.ChannelServices.LoggerServices;
public class ChannelContext : IDisposable
{
    public IPowerSupplyChannel Channel { get; }
    public CancellationTokenSource Cts { get; }
    public IMeasurementLogger Logger { get; }

    public ChannelContext(IPowerSupplyChannel channel,
                        CancellationTokenSource cts,
                        IMeasurementLogger logger)
    {
        Channel = channel;
        Cts = cts;
        Logger = logger;
    }

    public void Dispose()
    {
        Cts?.Cancel();
        Logger?.Dispose();
    }
}
