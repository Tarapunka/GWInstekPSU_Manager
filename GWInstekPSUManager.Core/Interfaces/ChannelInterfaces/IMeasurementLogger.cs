using GWInstekPSUManager.Core.Models;

namespace GWInstekPSUManager.Core.Interfaces.ChannelInterfaces;

public interface IMeasurementLogger : IDisposable
{
    void LogMeasurement(IPowerSupplyChannel channel, MeasureResponse measurement, TimeSpan elapsed);
    void StartNewLog();
}