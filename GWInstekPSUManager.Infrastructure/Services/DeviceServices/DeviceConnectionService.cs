using GWInstekPSUManager.Core.Events;
using GWInstekPSUManager.Core.Interfaces.ComPortsInterfaces;
using GWInstekPSUManager.Core.Interfaces.DeviceInterfaces;
using GWInstekPSUManager.Core.Models;
using GWInstekPSUManager.Infrastructure.Services.ChannelServices;
using System.IO.Ports;
using System.Runtime.CompilerServices;

namespace GWInstekPSUManager.Infrastructure.Services.DeviceServices;

public class DeviceConnectionService : IDeviceConnection
{
    private readonly ISerialPortService _serialPort;

    public bool IsConnected { get; private set; }
    public bool IsOpen => _serialPort.IsOpen;

    public event EventHandler<DeviceNotificationEventArgs> NotificationReceived;
    public event EventHandler<string> DataReceived;
    public event EventHandler<DeviceErrorEventArgs> ErrorOccurred;

    public DeviceConnectionService(ISerialPortService serialPort)
    {
        _serialPort = serialPort;
        _serialPort.DataReceived += OnDataReceived;
        //_serialPort.ErrorReceived += OnErrorReceived;
    }

    protected virtual void OnNotificationReceived(DeviceNotificationEventArgs args)
    {
        NotificationReceived?.Invoke(this, args);
    }

    private void OnErrorReceived(object sender, SerialErrorReceivedEventArgs e)
    {
        OnErrorOccurred(new DeviceErrorEventArgs(
            new Exception($"Serial port error: {e.EventType}")));
    }

    public async Task ConnectAsync(SerialPortSettings settings)
    {
        await _serialPort.OpenAsync(settings);
        IsConnected = true;
    }

    public async Task DisconnectAsync()
    {
        await _serialPort.CloseAsync();
        IsConnected = false;
    }

    private void OnDataReceived(object? sender, string data)
    {
        DataReceived?.Invoke(this, data);
    }

    public async Task SendCommandAsync(string command)
    {
        await _serialPort.SendCommandAsync(command);
    }

    public async Task<string> SendQueryAsync(string query)
    {
        return await _serialPort.QueryAsync(query);
    }

    protected virtual void OnErrorOccurred(DeviceErrorEventArgs e)
    {
        ErrorOccurred?.Invoke(this, e);
    }

    public void Dispose()
    {
        _serialPort.DataReceived -= OnDataReceived;
        _serialPort.Dispose();
    }

}