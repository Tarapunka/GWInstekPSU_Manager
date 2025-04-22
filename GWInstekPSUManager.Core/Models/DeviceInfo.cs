namespace GWInstekPSUManager.Core.Models;

public class DeviceInfo
{
    public string ConnectionName { get; set; }
    public string Name { get; set; }
    public string Model {  get; set; }
    public int ChannelCount { get; set; } = 4;
    public double MaxVoltage {  get; set; }
    public double MaxCurrent {  get; set; }
}
