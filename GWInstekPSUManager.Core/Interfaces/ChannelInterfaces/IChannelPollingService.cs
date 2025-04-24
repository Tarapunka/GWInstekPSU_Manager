using GWInstekPSUManager.Core.Events;

namespace GWInstekPSUManager.Core.Interfaces.ChannelInterfaces;

public interface IChannelPollingService : IDisposable
{
    event EventHandler<ChannelMeasurementEventArgs> MeasurementReceived;
    event Action<int> ChannelLimitExceeded;

    Task StartPollingAsync(int channelNumber, IPowerSupplyChannel channel);
    Task StopPollingAsync(int channelNumber);
    void ResetCapacityCounter(int channelNumber);
}