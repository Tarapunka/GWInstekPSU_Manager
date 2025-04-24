using GWInstekPSUManager.Core.Interfaces.ChannelInterfaces;

namespace GWInstekPSUManager.Core.Models;

public class ChannelCapacityCalculator : IChannelCapacityCalculator
{
    private DateTime? _lastMeasurementTime;
    private double _accumulatedCapacity;

    public double CalculateCapacity(double current, DateTime measurementTime)
    {
        if (_lastMeasurementTime.HasValue)
        {
            var timeSpan = measurementTime - _lastMeasurementTime.Value;
            _accumulatedCapacity += current * timeSpan.TotalHours;
        }
        _lastMeasurementTime = measurementTime;
        return _accumulatedCapacity;
    }

    public void Reset()
    {
        _accumulatedCapacity = 0;
        _lastMeasurementTime = null;
    }
}