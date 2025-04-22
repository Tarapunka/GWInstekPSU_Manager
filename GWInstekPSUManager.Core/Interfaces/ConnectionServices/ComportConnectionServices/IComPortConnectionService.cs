namespace GWInstekPSUManager.Core.Interfaces.ConnectionServices.ComportConnectionServices;

public interface IComPortConnectionService : IConnectionService
{
    Task ClearBuffersAsync();
}