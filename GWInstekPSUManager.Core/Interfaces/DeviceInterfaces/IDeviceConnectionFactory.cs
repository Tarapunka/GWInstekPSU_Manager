using GWInstekPSUManager.Core.Interfaces.ConnectionServices;

namespace GWInstekPSUManager.Core.Interfaces.DeviceInterfaces;

public interface IDeviceConnectionFactory
{
    IDeviceConnection Create(ConnectionType type, IConnectionSettings settings);
}
