using GWInstekPSUManager.Core.Interfaces.ChannelInterfaces;
using GWInstekPSUManager.Core.Interfaces.ConnectionServices;
using GWInstekPSUManager.Core.Interfaces.DeviceInterfaces;

namespace GWInstekPSUManager.Infrastructure.Services.ChannelServices;

public class ChannelControllerFactory : IChannelControllerFactory
{
    private IConnectionService _connection;
    private IDeviceProtocol _protocol;

    public ChannelControllerFactory(IConnectionService connection, IDeviceProtocol protocol)
    {
        _connection = connection;
        _protocol = protocol;
    }

    public IChannelController CreateChannelController()
    {
        return new ChannelControllerService(_connection,_protocol);
    }
}
