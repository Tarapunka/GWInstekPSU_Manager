using GWInstekPSUManager.Core.Events;
using GWInstekPSUManager.Core.Models;

namespace GWInstekPSUManager.Core.Interfaces.DeviceInterfaces;

public interface IDeviceConnection : IDisposable
{
    bool IsConnected { get; }
    Task ConnectAsync(SerialPortSettings settings);
    Task DisconnectAsync();

    Task SendCommandAsync(string command);

    Task<string> SendQueryAsync(string query);

    event EventHandler<DeviceNotificationEventArgs> NotificationReceived;
    event EventHandler<string> DataReceived;
    event EventHandler<DeviceErrorEventArgs> ErrorOccurred;
}