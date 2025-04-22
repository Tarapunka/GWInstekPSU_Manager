using CommunityToolkit.Mvvm.ComponentModel;
using GWInstekPSUManager.Core.Interfaces.ConnectionServices.ComportConnectionServices;
using GWInstekPSUManager.Core.Interfaces.ConnectionServices;
using System.Collections.ObjectModel;
using System.IO.Ports;
using GWInstekPSUManager.Infrastructure.Services.ConectionServices;

namespace GWInstekPSUManager.ViewModels;

public class ConnectionSettingsViewModel : ObservableObject
{
    private ConnectionType _selectedConnectionType = ConnectionType.ComPort;
    private IComPortSettings _comPortSettings;

    public ConnectionType SelectedConnectionType
    {
        get => _selectedConnectionType;
        set => SetProperty(ref _selectedConnectionType, value);
    }

    public IComPortSettings ComPortSettings
    {
        get => _comPortSettings;
        set => SetProperty(ref _comPortSettings, value);
    }

    // Список доступных COM портов
    public ObservableCollection<string> AvailablePorts { get; } = new();

    // Список скоростей BaudRate
    public ObservableCollection<int> AvailableBaudRates { get; } = new()
    {
        9600, 19200, 38400, 57600, 115200
    };

    // Инициализация доступных портов
    public async Task RefreshAvailablePortsAsync()
    {
        AvailablePorts.Clear();
        var ports = await Task.Run(() => SerialPort.GetPortNames());
        foreach (var port in ports)
        {
            AvailablePorts.Add(port);
        }
    }
}

