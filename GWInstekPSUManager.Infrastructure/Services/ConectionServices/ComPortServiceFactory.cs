using GWInstekPSUManager.Core.Interfaces.ConnectionServices;
using GWInstekPSUManager.Core.Interfaces.ConnectionServices.ComportConnectionServices;
using System.IO.Ports;

namespace GWInstekPSUManager.Infrastructure.Services.ConectionServices;

public class ComPortServiceFactory : IConnectionServiceFactory<IComPortSettings>
{
    public IConnectionService CreateConnectionService(IComPortSettings settings)
    {
        return new ComPortService(settings);
    }
}