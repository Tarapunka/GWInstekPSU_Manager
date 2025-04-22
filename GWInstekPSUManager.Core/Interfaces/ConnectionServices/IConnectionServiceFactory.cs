using GWInstekPSUManager.Core.Interfaces.ConnectionServices.ComportConnectionServices;

namespace GWInstekPSUManager.Core.Interfaces.ConnectionServices;

public interface IConnectionServiceFactory<in TSettings> where TSettings : IConnectionSettings
{
    IConnectionService CreateConnectionService(TSettings settings);
}