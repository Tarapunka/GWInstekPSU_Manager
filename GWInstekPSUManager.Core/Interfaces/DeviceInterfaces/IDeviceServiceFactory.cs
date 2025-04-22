using GWInstekPSUManager.Core.Interfaces.ComPortsInterfaces;
using GWInstekPSUManager.Core.Interfaces.ConnectionServices;
using GWInstekPSUManager.Core.Models;

namespace GWInstekPSUManager.Core.Interfaces.DeviceInterfaces;

public interface IDeviceServiceFactory
{
    IDeviceService CreateDeviceFacade(IConnectionService connectionService);
}

