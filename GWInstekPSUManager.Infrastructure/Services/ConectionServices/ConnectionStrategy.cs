using GWInstekPSUManager.Core.Interfaces.ConnectionServices;
using GWInstekPSUManager.Core.Interfaces.ConnectionServices.ComportConnectionServices;

namespace GWInstekPSUManager.Infrastructure.Services.ConectionServices;

public class ConnectionStrategy : IConnectionStrategy
{
    public IConnectionService CreateConnectionService(ConnectionType type, IConnectionSettings connectionSettings)
    {
        switch (type)
        {
            case ConnectionType.ComPort:
                if (connectionSettings is not IComPortSettings comSettings)
                    throw new ArgumentException("Invalid settings type for COM port");
                return new ComPortServiceFactory().CreateConnectionService(comSettings);

            default:
                throw new NotSupportedException($"Connection type {type} is not supported");
        }
    }
}