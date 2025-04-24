namespace GWInstekPSUManager.Core.Interfaces.ChannelInterfaces;

public interface IChannelCapacityCalculator
{
    double CalculateCapacity(double current, DateTime measurementTime);
    void Reset();
}