
namespace GWInstekPSUManager.Core.Interfaces.ChannelInterfaces;
public interface IChannelManager
{
    event EventHandler<IPowerSupplyChannel>? ChannelAdded;
    event EventHandler<int>? ChannelRemoved;

    Task AddChannelAsync();
    void Dispose();
    void RemoveChannel(int channelNumber);
    Task SetLoadModeAsync(int channelNumber, string mode);
    void StartPollingChannel(int channelNumber);
    void StopPollingChannel(int channelNumber);
}
