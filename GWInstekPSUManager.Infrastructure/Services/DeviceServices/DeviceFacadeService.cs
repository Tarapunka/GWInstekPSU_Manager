using GWInstekPSUManager.Core.Events;
using GWInstekPSUManager.Core.Interfaces.ChannelInterfaces;
using GWInstekPSUManager.Core.Interfaces.ConnectionServices;
using GWInstekPSUManager.Core.Interfaces.DeviceInterfaces;
using GWInstekPSUManager.Core.Models;
using GWInstekPSUManager.Infrastructure.Services.ChannelServices;
using System.Globalization;

namespace GWInstekPSUManager.Infrastructure.Services.DeviceServices
{
    public class DeviceFacadeService : IDeviceService
    {
        private readonly IConnectionService _connection;
        private readonly IChannelController _channelController;
        private readonly IDeviceSystem _system;
        private readonly IDeviceProtocol _protocol;
        private bool _disposed;

        public bool IsConnected => _connection.IsConnected;
        public string DeviceModel { get; private set; }

        public IConnectionService Connection { get => _connection; }

        public event EventHandler<string> DataReceived;
        public event EventHandler<DeviceStatusEventArgs> StatusChanged;
        public event EventHandler<DeviceErrorEventArgs> ErrorOccurred;
        public event EventHandler<DeviceNotificationEventArgs> NotificationReceived;

        public DeviceFacadeService(
            IConnectionService connection,
            IChannelController channels,
            IDeviceSystem system,
            IDeviceProtocol protocol)
        {
            _connection = connection;
            _channelController = channels;
            _system = system;
            _protocol = protocol;

            _connection.DataReceived += OnDataReceived;
            //_connection.NotificationReceived += OnConnectionNotificationReceived;
        }

        private void OnConnectionNotificationReceived(object sender, DeviceNotificationEventArgs e)
        {
            NotificationReceived?.Invoke(this, e);
        }

        #region Connection Management
        public async Task InitializeAsync(SerialPortSettings portSettings)
        {

            await ExecuteSafeAsync(async () =>
            {
                DeviceModel = await _system.GetDeviceInfoAsync();
                OnStatusChanged($"Connected to {DeviceModel}");
            }, "Device initialization");
        }

        public async Task<(IPowerSupplyChannel newChannel, string LoadState)> InitializeChannelsAsync(int channel)
        {
            try
            {
                return await _channelController.InitializeChannelsAsync(channel);
            }
            catch
            {
                throw new Exception($"the device does not support the channel number {channel}");
            }
        }


        public async Task DisconnectAsync()
        {
            await ExecuteSafeAsync(async () =>
            {
                await _connection.DisconnectAsync();
                OnStatusChanged("Disconnected");
            }, "Device disconnection");
        }
        #endregion

        #region System Commands
        public async Task ResetDeviceAsync()
        {
            await ExecuteSafeAsync(() => _system.ResetAsync(), "Device reset");
        }

        public async Task BeepAsync()
        {
            await ExecuteSafeAsync(() => _system.BeepAsync(), "Device beep");
        }

        public async Task<string> GetDeviceInfoAsync()
        {
            return await ExecuteSafeAsync(() => _system.GetDeviceInfoAsync(), "Get device info");
        }
        #endregion

        #region Channel Operations
        public async Task SetChannelVoltageAsync(int channel, double voltage)
        {
            await ExecuteSafeAsync(() => _channelController.SetVoltageAsync(channel, voltage),
                $"Set voltage {voltage}V on CH{channel}");
        }

        public async Task<double> GetChannelVoltageAsync(int channel)
        {
            return await ExecuteSafeAsync(() => _channelController.GetVoltageAsync(channel),
                $"Get voltage from CH{channel}");
        }

        public async Task SetChannelCurrentAsync(int channel, double current)
        {
            await ExecuteSafeAsync(() => _channelController.SetCurrentAsync(channel, current),
                $"Set current {current}A on CH{channel}");
        }

        public async Task<double> GetCurrentAsync(int channel)
        {
            return await ExecuteSafeAsync(() => _channelController.GetCurrentAsync(channel),
                $"Get current from CH{channel}");
        }

        public async Task<double> GetPowerAsync(int channel)
        {
            return await ExecuteSafeAsync(async () =>
            {
                var voltage = await _channelController.GetVoltageAsync(channel);
                var current = await _channelController.GetCurrentAsync(channel);
                return voltage * current;
            }, $"Get power from CH{channel}");
        }

        public async Task<bool> TurnChannelAsync(int channel)
        {
            return await ExecuteSafeAsync(async () =>
            {
                return await _channelController.SwitchEnableOutputAsync(channel);

            }, $"Toggle CH{channel} output");
        }

        public async Task SetVsetAsync(int channel, double vset)
        {
            await ExecuteSafeAsync(() => _channelController.SetVoltageAsync(channel, vset),
                $"Set Vset {vset} V to CH{channel}");
        }

        public async Task SetIsetAsync(int channel, double iset)
        {
            await ExecuteSafeAsync(() => _channelController.SetCurrentAsync(channel, iset),
                $"Set Iset {iset} A to CH{channel}");
        }

        #endregion

        #region Protection Settings
        public async Task<bool> SwitchOVPModeAsync(int channel)
        {
            return await ExecuteSafeAsync(async () =>
            {
                return await _channelController.SwitchOVPModeAsync(channel);
            },
                $"Toggle OVP on CH{channel}");
        }

        public async Task<bool> GetOVPModeAsync(int channel)
        {
            return await ExecuteSafeAsync(async () =>
            {
                var command = _protocol.Query.GetOVPMode(channel);
                var response = await _connection.SendQueryAsync(command);
                return response == "ON";
            }, $"Get OVP status from CH{channel}");
        }

        public async Task SetOVPValueAsync(int channel, double ovpValue)
        {
            await ExecuteSafeAsync(() =>
            {
                var command = _protocol.Build.SetOVPValue(channel, ovpValue);
                return _connection.SendCommandAsync(command);
            }, $"Set OVP value {ovpValue}V on CH{channel}");
        }

        public async Task<double> GetOVPValueAsync(int channel)
        {
            return await ExecuteSafeAsync(async () =>
            {
                var command = _protocol.Query.GetOVPValue(channel);
                var response = await _connection.SendQueryAsync(command);
                return double.Parse(response, CultureInfo.InvariantCulture);
            }, $"Get OVP value from CH{channel}");
        }

        public async Task<bool> SwitchOCPModeAsync(int channel)
        {
            return await ExecuteSafeAsync(async () =>
            {
                return await _channelController.SwitchOCPModeAsync(channel);
            },
                $"Toggle OCP on CH{channel}");
        }

        public async Task<bool> GetOCPModeAsync(int channel)
        {
            return await ExecuteSafeAsync(() => _channelController.GetOCPStatusAsync(channel),
                $"Get OCP status from CH{channel}");
        }

        public async Task SetOCPValueAsync(int channel, double ocpValue)
        {
            await ExecuteSafeAsync(() => _channelController.SetOCPValueAsync(channel, ocpValue),
                $"Set OCP value {ocpValue}A on CH{channel}");
        }

        public async Task<double> GetOCPValueAsync(int channel)
        {
            return await ExecuteSafeAsync(() => _channelController.GetOCPValueAsync(channel),
                $"Get OCP value from CH{channel}");
        }
        #endregion

        #region Load Modes
        public async Task<bool> SetChannelLoadModeAsync(int channel, string mode)
        {
            return await ExecuteSafeAsync(async () =>
            {
                return await _channelController.SetChannelLoadModeAsync(channel, mode);
            },
                $"Set {mode} mode on CH{channel}");
        }

        public async Task<string> GetChannelModeStatusAsync(int channel)
        {
            return await ExecuteSafeAsync(() => _channelController.GetLoadModeAsync(channel),
                $"Get mode status from CH{channel}");
        }
        #endregion

        #region Parallel/Series Modes
        public async Task<bool> SetParallelModeAsync(int channel)
        {
            return await ExecuteSafeAsync(async () =>
            {

                var mode = await _connection.SendQueryAsync(_protocol.Query.GetModeStatus(channel)); // Запрос на текущий режим канала
                bool status = mode == "PAR"; // Сравниваем и создаем булевое значение
                var command = _protocol.Build.SetParallelMode(channel, !status); // Получаем строку комманды на переключение режима
                await _connection.SendCommandAsync(command);

                mode = await _connection.SendQueryAsync(_protocol.Query.GetModeStatus(channel));
                return status = mode == "PAR";
            }, $"parallel mode");
        }

        public async Task<bool> SetSeriesModeAsync(int channel)
        {
            return await ExecuteSafeAsync(async () =>
            {
                var mode = await _connection.SendQueryAsync(_protocol.Query.GetModeStatus(channel)); // Запрос на текущий режим канала
                bool status = mode == "SER"; // Сравниваем и создаем булевое значение
                var command = _protocol.Build.SetSeriesMode(channel, !status); // Получаем строку комманды на переключение режима
                await _connection.SendCommandAsync(command);

                mode = await _connection.SendQueryAsync(_protocol.Query.GetModeStatus(channel));
                return status = mode == "SER";

            }, $"series mode");
        }
        #endregion

        #region Measurements
        public async Task<MeasureResponse> GetMeasureChannelAsync(int channel)
        {
            return await ExecuteSafeAsync(() => _channelController.GetMeasureAsync(channel),
                $"Get measurement from CH{channel}");
        }

        #endregion

        #region Event Handling
        private void OnDataReceived(object sender, string data)
        {
            DataReceived?.Invoke(this, data);

            if (_protocol.TryParseNotification(data, out var notification))
            {
                OnStatusChanged($"Device notification: {notification.Message}");
            }
        }

        protected virtual void OnStatusChanged(string message)
        {
            StatusChanged?.Invoke(this, new DeviceStatusEventArgs(message));
        }

        protected virtual void OnErrorOccurred(string message, Exception ex = null)
        {
            ErrorOccurred?.Invoke(this, new DeviceErrorEventArgs(ex ?? new Exception(message)));
        }
        #endregion

        #region Helper Methods
        private async Task ExecuteSafeAsync(Func<Task> action, string operationName)
        {
            try
            {
                await action();
                OnStatusChanged($"{operationName} completed");
            }
            catch (Exception ex)
            {
                OnErrorOccurred($"{operationName} failed: {ex.Message}", ex);
                throw;
            }
        }

        private async Task<T> ExecuteSafeAsync<T>(Func<Task<T>> action, string operationName)
        {
            try
            {
                var result = await action();
                OnStatusChanged($"{operationName} completed");
                return result;
            }
            catch (Exception ex)
            {
                OnErrorOccurred($"{operationName} failed: {ex.Message}", ex);
                throw;
            }
        }
        #endregion

        #region IDisposable
        public void Dispose()
        {
            if (_disposed) return;

            _connection.DataReceived -= OnDataReceived;
            //_connection.NotificationReceived -= OnConnectionNotificationReceived;
            _connection.Dispose();
            _disposed = true;
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}