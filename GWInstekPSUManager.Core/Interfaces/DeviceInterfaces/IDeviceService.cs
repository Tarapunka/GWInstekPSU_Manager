
using GWInstekPSUManager.Core.Events;
using GWInstekPSUManager.Core.Interfaces.ChannelInterfaces;
using GWInstekPSUManager.Core.Interfaces.ConnectionServices;
using GWInstekPSUManager.Core.Models;

namespace GWInstekPSUManager.Core.Interfaces.DeviceInterfaces;

public interface IDeviceService : IDisposable
{
    bool IsConnected { get; }
    string DeviceModel { get; }

    IConnectionService Connection {get;}

    event EventHandler<string> DataReceived;

    event EventHandler<DeviceNotificationEventArgs> NotificationReceived;
    event EventHandler<DeviceStatusEventArgs> StatusChanged; // Уведомление об изменении статуса
    event EventHandler<DeviceErrorEventArgs> ErrorOccurred;

    Task InitializeAsync(SerialPortSettings portSettings);
    Task<(IPowerSupplyChannel newChannel, string LoadState)> InitializeChannelsAsync(int channel);
    Task ResetDeviceAsync();

    // Channel operations

    #region Set Command
    Task SetChannelVoltageAsync(int channel, double voltage);

    Task SetChannelCurrentAsync (int channel, double current);

    Task<bool> SetChannelLoadModeAsync(int channel, string chanelMode);

    Task<bool> SwitchOVPModeAsync(int channel);

    Task<bool> SwitchOCPModeAsync(int channel);

    Task SetOVPValueAsync(int channel, double ovpValue);

    Task SetOCPValueAsync(int channel, double ocpValue);

    Task<bool> SetParallelModeAsync(int channel);

    Task<bool> SetSeriesModeAsync(int channel);

    Task SetVsetAsync(int channel, double vset);

    Task SetIsetAsync(int channel, double iset);


    #endregion


    #region Get Command
    Task<double> GetChannelVoltageAsync(int channel);

    Task<double> GetCurrentAsync(int channel);

    Task<double> GetPowerAsync(int channel);

    Task<bool> GetOVPModeAsync(int channel);

    Task<bool> GetOCPModeAsync(int channel);

    Task<double> GetOVPValueAsync(int channel);

    Task<double> GetOCPValueAsync(int channel);

    Task<string>GetChannelModeStatusAsync(int channel);
    #endregion

    Task<bool> TurnChannelAsync(int channel);

    Task<MeasureResponse> GetMeasureChannelAsync(int channel);

    Task DisconnectAsync();

    // System commands
    Task<string> GetDeviceInfoAsync();
    Task BeepAsync();
}