using GWInstekPSUManager.Core.Interfaces.ComPortsInterfaces;
using GWInstekPSUManager.Core.Interfaces.DeviceInterfaces;

namespace GWInstekPSUManager.Infrastructure.Services.ComPortServices;

public class SerialPortServiceFactory : ISerialPortServiceFactory
{
    public ISerialPortService Create()
    {
        return new SerialPortService();
    }
}