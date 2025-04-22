using GWInstekPSUManager.Core.Interfaces.ConnectionServices;
using GWInstekPSUManager.Core.Interfaces.DeviceInterfaces;

namespace GWInstekPSUManager.Infrastructure.Services.DeviceServices;

public class DeviceSystemService : IDeviceSystem
{
    private readonly IConnectionService _connection;

    public DeviceSystemService(IConnectionService connection)
    {
        _connection = connection;
    }

    public async Task ResetAsync()
    {
        await _connection.SendCommandAsync("*RST");
    }

    public async Task BeepAsync()
    {
        await _connection.SendCommandAsync("SYST:BEEP");
    }

    public async Task<string> GetDeviceInfoAsync()
    {
        return await _connection.SendQueryAsync("*IDN?");
    }
}