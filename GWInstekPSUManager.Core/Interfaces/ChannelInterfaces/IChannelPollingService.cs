using GWInstekPSUManager.Core.Events;

namespace GWInstekPSUManager.Core.Interfaces.ChannelInterfaces;

public interface IChannelPollingService : IDisposable
{
    Task StartPollingAsync(int channelNumber, IPowerSupplyChannel powerSupplyChannel);
    Task StopPollingAsync(int channelNumber);
    event EventHandler<ChannelMeasurementEventArgs> MeasurementReceived;
    event Action<int> ChannelLimitExceeded;
}
