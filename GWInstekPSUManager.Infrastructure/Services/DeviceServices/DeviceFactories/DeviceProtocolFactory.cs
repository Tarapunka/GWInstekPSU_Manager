using GWInstekPSUManager.Core.Interfaces.ComPortsInterfaces;
using GWInstekPSUManager.Core.Interfaces.DeviceInterfaces;
using Microsoft.Extensions.Logging;

namespace GWInstekPSUManager.Infrastructure.Services.DeviceServices.DeviceFactories;

public class DeviceProtocolFactory : IDeviceProtocolFactory
{
    public IDeviceProtocol Create()
    {
        return new DeviceProtocol();
    }
}
