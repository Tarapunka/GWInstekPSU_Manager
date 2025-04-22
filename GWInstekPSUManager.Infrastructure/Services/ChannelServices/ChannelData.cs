namespace GWInstekPSUManager.Infrastructure.Services.ChannelServices;

public class ChannelData
{
    public int ChannelNumber { get; set; }
    public double Voltage { get; set; }
    public double Current { get; set; }
    public double Power { get; set; }
    public double Capacity { get; set; }
    public double CurrentLimit { get; set; }
    public double VoltageLimit { get; set; }
    public string Mode { get; set; }
    public DateTime StartTime { get; set; }
    public TimeSpan CurrentTime { get; set; }
    public bool IsEnabled { get; set; }
    public bool IsCalibrated { get; set; }
}
