using GWInstekPSUManager.Core.Models;
using System;

namespace GWInstekPSUManager.Core.Events;

public class ChannelMeasurementEventArgs : EventArgs
{
    public int ChannelNumber { get; }
    public MeasureResponse Measurements { get; }
    public TimeSpan ElapsedTime { get; }
    public double CapacityAh { get; } // Добавляем емкость

    public ChannelMeasurementEventArgs(
        int channelNumber,
        MeasureResponse measurements,
        TimeSpan elapsedTime,
        double capacityAh = 0)
    {
        ChannelNumber = channelNumber;
        Measurements = measurements;
        ElapsedTime = elapsedTime;
        CapacityAh = capacityAh;
    }
}