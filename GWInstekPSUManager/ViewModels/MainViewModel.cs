using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using GWInstekPSUManager.Core.Interfaces.ComPortsInterfaces;
using GWInstekPSUManager.Core.Interfaces.DeviceInterfaces;
using GWInstekPSUManager.Infrastructure.Services.ChannelServices;
using GWInstekPSUManager.Core.Interfaces.ConnectionServices;
using System.Diagnostics;
using GWInstekPSUManager.Core.Models;
using System.Windows;
using System.IO.Ports;
using GWInstekPSUManager.Infrastructure.Services.ConectionServices;
using System.Threading.Channels;

namespace GWInstekPSUManager.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IDeviceServiceFactory _deviceServiceFactory;
    private readonly IPortDiscoverer _portDiscoverer;
    private readonly IConnectionStrategy _connectionStrategy;

    private ObservableCollection<IConnectionService> _connectionServices = new();

    private ObservableCollection<DeviceViewModel> _deviceViewModels = new();
    private ObservableCollection<SerialPortInfo> _availablePorts = new();

    [ObservableProperty]
    private ChannelManagerViewModel _channelManager;

    private int _baudRate = 115200;
    private int _dataBits = 8;


    private DeviceViewModel? _selectedDeviceViewModel;
    private SerialPortInfo? _selectedPort;


    private string _statusMessage = "Ready";




    public MainViewModel(IDeviceServiceFactory deviceServiceFactory, IPortDiscoverer portDiscoverer, IConnectionStrategy connectionStrategy)
    {
        _deviceServiceFactory = deviceServiceFactory;
        _portDiscoverer = portDiscoverer;
        _connectionStrategy = connectionStrategy;

        _deviceViewModels = new ObservableCollection<DeviceViewModel>();
        _channelManager = new ChannelManagerViewModel(_deviceViewModels);

        InitializeAsync();

    }

    private async Task InitializeAsync()
    {
        try
        {
            StatusMessage = "Initializing...";
            var ports = _portDiscoverer.GetAvailablePorts();

            Application.Current.Dispatcher.Invoke(() =>
            {
                AvailablePorts.Clear();
                foreach (var port in ports) AvailablePorts.Add(port);
                StatusMessage = ports.Any() ? $"Найдено портов: {ports.Count()}" : "Порты не найдены";
                SelectedPort = ports.FirstOrDefault(p => p.IsGwInstekDevice);
            });
            StatusMessage = "Ready";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Initialization error: {ex.Message}";
            Debug.WriteLine(ex);
        }
    }

    [RelayCommand]
    private async Task ConnectAsync()
    {
        try
        {
            if (string.IsNullOrEmpty(SelectedPort!.PortName))
                return;

            var settings = new ComPortSettings
            {
                PortName = SelectedPort.PortName,
                BaudRate = BaudRate,
                DataBits = DataBits,
                Parity = Parity.None,
                StopBits = StopBits.One,
                ReadTimeout = 10000 // тестирую скорость ответа. Пока оптимальное
            };
            StatusMessage = "Connecting...";

            var connection = _connectionStrategy.CreateConnectionService(ConnectionType.ComPort, settings);

            var existingConnection = ConnectionServices.FirstOrDefault(c => c.ConnectionName == connection.ConnectionName);
            if (existingConnection != null)
            {
                // Находим устройство, которое использует это подключение
                var existingDevice = DeviceViewModels.FirstOrDefault(d =>
                    d.DeviceService.Connection.ConnectionName == connection.ConnectionName);

                if (existingDevice != null)
                {
                    SelectedDeviceViewModel = existingDevice;
                    await SelectedDeviceViewModel!.DeviceService.DisconnectAsync();
                    RemoveDevice();
                }
            }
            await connection.ConnectAsync();

            if (!connection.IsConnected)
            {
                StatusMessage = "Connection failed";
                return;
            }
            ConnectionServices.Add(connection);

            var deviceService = _deviceServiceFactory.CreateDeviceFacade(connection);
            var channelPollingService = new ChannelPollingService(deviceService);
            var deviceViewModel = new DeviceViewModel(deviceService, _portDiscoverer, channelPollingService);

            if (!DeviceViewModels.Contains(deviceViewModel))
                DeviceViewModels.Add(deviceViewModel);

            SelectedDeviceViewModel = deviceViewModel;

            StatusMessage = $"Connected to {SelectedPort.PortName}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Connection error: {ex.Message}";
            Debug.WriteLine(ex);
        }
    }

    [RelayCommand]
    private void RemoveDevice()
    {
        if (DeviceViewModels.Count <= 0 && SelectedDeviceViewModel != null) return;

        try
        {
            string deviceName = SelectedDeviceViewModel!.DeviceName;
            StatusMessage = $"Removing device {deviceName}...";

            var connection = ConnectionServices.FirstOrDefault(
                c => c == SelectedDeviceViewModel.DeviceService.Connection);

            connection!.DisconnectAsync();
            connection?.Dispose();
            ConnectionServices.Remove(connection!);

            SelectedDeviceViewModel.Dispose();
            DeviceViewModels.Remove(SelectedDeviceViewModel);

            SelectedDeviceViewModel = DeviceViewModels.LastOrDefault();

            StatusMessage = $"Device {deviceName} removed";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Remove error: {ex.Message}";
            Debug.WriteLine(ex);
        }
    }

    [RelayCommand]
    private void ClearDevices()
    {
        try
        {
            StatusMessage = "Clearing all devices...";

            foreach (var connection in ConnectionServices)
            {
                connection.Dispose();
            }
            ConnectionServices.Clear();

            foreach (var deviceViewModel in DeviceViewModels)
            {
                deviceViewModel.Dispose();
            }
            DeviceViewModels.Clear();

            SelectedDeviceViewModel = null;
            StatusMessage = "All devices cleared";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Clear error: {ex.Message}";
            Debug.WriteLine(ex);
        }
    }

    [RelayCommand]
    private async Task DisconnectAsync()
    {
        try
        {
            await SelectedDeviceViewModel!.DeviceService.DisconnectAsync();
            StatusMessage = !SelectedDeviceViewModel!.DeviceService.IsConnected ? $"{SelectedDeviceViewModel.DeviceName} Отключено" : $"{SelectedDeviceViewModel.DeviceName} Подключено";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка отключения: {ex.Message}";
        }
    }


    [RelayCommand]
    private async Task DisconnectAllAsync()
    {
        try
        {
            StatusMessage = "Disconnecting all devices...";

            foreach (var connection in ConnectionServices)
            {
                await connection.DisconnectAsync();
            }

            StatusMessage = "All devices disconnected";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Disconnect error: {ex.Message}";
            Debug.WriteLine(ex);
        }
    }


    public ObservableCollection<DeviceViewModel> DeviceViewModels { get => _deviceViewModels; set => SetProperty(ref _deviceViewModels, value); }
    public ObservableCollection<SerialPortInfo> AvailablePorts { get => _availablePorts; set => SetProperty(ref _availablePorts, value); }
    public ObservableCollection<IConnectionService> ConnectionServices { get => _connectionServices; set => SetProperty(ref _connectionServices, value); }
    public DeviceViewModel? SelectedDeviceViewModel { get => _selectedDeviceViewModel; set => SetProperty(ref _selectedDeviceViewModel, value); }
    public string StatusMessage { get => _statusMessage; set => SetProperty(ref _statusMessage, value); }
    public SerialPortInfo? SelectedPort { get => _selectedPort; set => SetProperty(ref _selectedPort, value); }
    public int BaudRate { get => _baudRate; set => SetProperty(ref _baudRate, value); }
    public int DataBits { get => _dataBits; set => SetProperty(ref _dataBits, value); }
    public int[] AvailableBaudRates { get; } = [110, 300, 1200, 2400, 4800, 9600, 19200, 38400, 57600, 115200];
    public int[] AvailableDataBits { get; } = [5, 6, 7, 8];
}
