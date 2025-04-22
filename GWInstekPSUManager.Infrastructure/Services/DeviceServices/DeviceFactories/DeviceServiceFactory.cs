using GWInstekPSUManager.Core.Interfaces.ChannelInterfaces;
using GWInstekPSUManager.Core.Interfaces.ConnectionServices;
using GWInstekPSUManager.Core.Interfaces.DeviceInterfaces;
using GWInstekPSUManager.Infrastructure.Services.ChannelServices;

namespace GWInstekPSUManager.Infrastructure.Services.DeviceServices.DeviceFactories;

public class DeviceServiceFactory : IDeviceServiceFactory
{
    private readonly IDeviceProtocolFactory _protocolFactory;

    public DeviceServiceFactory(IDeviceProtocolFactory protocolFactory, IChannelControllerFactory channelControllerFactory)
    {
        _protocolFactory = protocolFactory;
    }

    public IDeviceService CreateDeviceFacade(IConnectionService connectionService)
    {
        var protocol = _protocolFactory.Create();                

        var channelController = new ChannelControllerFactory(connectionService, protocol).CreateChannelController();

        var system = new DeviceSystemService(connectionService);

        return new DeviceFacadeService(connectionService, channelController, system, protocol);
    }
}
