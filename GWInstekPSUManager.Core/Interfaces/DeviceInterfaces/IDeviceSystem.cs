namespace GWInstekPSUManager.Core.Interfaces.DeviceInterfaces;

public interface IDeviceSystem
{
    Task ResetAsync();
    Task BeepAsync();
    Task<string> GetDeviceInfoAsync();
}