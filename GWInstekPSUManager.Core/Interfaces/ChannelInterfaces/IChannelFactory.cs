namespace GWInstekPSUManager.Core.Interfaces.ChannelInterfaces;

public interface IChannelFactory
{
    IPowerSupplyChannel CreateDefaultChannel();
    IPowerSupplyChannel CreateFromFile(string filePath);
    Task<IPowerSupplyChannel> CreateFromFileAsync(string filePath);
    IPowerSupplyChannel CreateHighCurrentChannel(double maxCurrent);
    IPowerSupplyChannel CreateHighVoltageChannel(double maxVoltage);
}