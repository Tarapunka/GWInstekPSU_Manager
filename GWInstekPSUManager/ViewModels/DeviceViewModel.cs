using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GWInstekPSUManager.Core.Events;
using GWInstekPSUManager.Core.Interfaces.ChannelInterfaces;
using GWInstekPSUManager.Core.Interfaces.ComPortsInterfaces;
using GWInstekPSUManager.Core.Interfaces.DeviceInterfaces;
using GWInstekPSUManager.Core.Models;
using GWInstekPSUManager.Infrastructure.Services.ChannelServices;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Threading.Channels;
using System.Windows;

namespace GWInstekPSUManager.ViewModels
{
    public partial class DeviceViewModel : ObservableObject, IDisposable
    {
        // Поля
        private readonly IDeviceService _deviceManager;
        private readonly IPortDiscoverer _portDiscoverer;
        private readonly IChannelPollingService _channelPollingService;
        private readonly Dictionary<int, CancellationTokenSource> _cancellationTokens = new();
        private bool _disposed;

        // Свойства
        #region Observable Properties

        [ObservableProperty]
        private bool _isConnected;

        [ObservableProperty]
        private bool _isBusy = false;

        [ObservableProperty]
        private ObservableCollection<PowerSupplyChannel> _channels = new();

        [ObservableProperty]
        private string _statusMessage = "Disconnected";

        [ObservableProperty]
        private string _deviceName = "Unknown device";

        //[ObservableProperty]
        //private string _turnButtonText = "Включить канал";


        
        private PowerSupplyChannel _selectedChannel;

        public IDeviceService DeviceService { get => _deviceManager; }
        #endregion

        // Команды
        #region Commands

        [RelayCommand]
        private async Task TurnChannelAsync()
        {
            if (SelectedChannel == null) return;
            if (SelectedChannel.IsParallelOn || SelectedChannel.IsSeriesOn)
            {
                await TurnDualChannelAsync();
                return;
            }

            await TurnChannelAsync(SelectedChannel.ChannelNumber);
        }

        public async Task TurnChannelAsync(int channelNumber)
        {
            var channel = Channels.FirstOrDefault(c => c.ChannelNumber == channelNumber);
            if (channel == null) return;

            bool newState = await _deviceManager.TurnChannelAsync(channelNumber);
            channel.IsEnabled = newState;

            if (newState)
            {
                _channelPollingService.ChannelLimitExceeded += async (num) =>
                {
                    await _deviceManager.TurnChannelAsync(num);
                    channel.IsEnabled = false;
                };
                await _channelPollingService.StartPollingAsync(channelNumber, channel);
            }
            else
            {
                await _channelPollingService.StopPollingAsync(channelNumber);
            }
        }

        private async Task TurnDualChannelAsync()
        {
            if (Channels.Count <= 1)
                await AddChannel();

            bool newState = await _deviceManager.TurnChannelAsync(Channels.First().ChannelNumber);

            foreach (var channel in Channels.Where(c => c.ChannelNumber <= 2))
            {
                channel.IsEnabled = newState;
                if (newState)
                    await _channelPollingService.StartPollingAsync(channel.ChannelNumber, channel);
                else
                    await _channelPollingService.StopPollingAsync(channel.ChannelNumber);
            }
        }

        private bool CanTurnChannel() => !IsBusy && SelectedChannel != null;
        partial void OnIsBusyChanged(bool value) => TurnChannelCommand.NotifyCanExecuteChanged();
        //partial void OnSelectedChannelChanged(PowerSupplyChannel value) => TurnChannelCommand.NotifyCanExecuteChanged();

        [RelayCommand]
        private async Task AddChannel()
        {
            try
            {
                await InitializeChannels(Channels.Count+1);
                SelectedChannel = Channels[^1];
            }
            catch (Exception ex)
            {
                HandleError($"Ошибка при добавлении канала: {ex.Message}");
            }
        }

        [RelayCommand]
        private void RemoveChannel()
        {
            if (SelectedChannel == null) return;
            var channel = Channels.FirstOrDefault(c => c.ChannelNumber == SelectedChannel.ChannelNumber);
            if (channel != null) Channels.Remove(channel);
            SelectedChannel = Channels.LastOrDefault();
        }

        [RelayCommand]
        private async Task SetCCLoadMode()
        {
            if (SelectedChannel == null)
                return;
            bool state = await _deviceManager.SetChannelLoadModeAsync(SelectedChannel.ChannelNumber, "CC");
            SelectedChannel.ChangeLoadMode("CC", state);
        }

        [RelayCommand]
        private async Task SetCVLoadMode()
        {
            if (SelectedChannel == null)
                return;
            bool state = await _deviceManager.SetChannelLoadModeAsync(SelectedChannel.ChannelNumber, "CV");
            await SelectedChannel.ChangeLoadModeAsync("CV", state);
        }

        [RelayCommand]
        private async Task SetCRLoadMode()
        {
            if (SelectedChannel == null)
                return;
            bool state = await _deviceManager.SetChannelLoadModeAsync(SelectedChannel.ChannelNumber, "CR");
            await SelectedChannel.ChangeLoadModeAsync("CR", state);
        }

        [RelayCommand]
        private async Task SetVset(double vset)
        {
            if (SelectedChannel == null) return;
            await _deviceManager.SetVsetAsync(SelectedChannel.ChannelNumber, SelectedChannel.Vset); // Костыль! Исправить!
        }


        [RelayCommand]
        private async Task SetIset(double iset)
        {
            if (SelectedChannel == null) return;
            await _deviceManager.SetIsetAsync(SelectedChannel.ChannelNumber, SelectedChannel.Iset); // Костыль! Исправить!
        }


        [RelayCommand]
        private async Task SetParallelModeAsync()
        {
            if(SelectedChannel == null || SelectedChannel.ChannelNumber>2) return;

            if(Channels.Count<=1)
                await AddChannel();

            bool state = await _deviceManager.SetParallelModeAsync(SelectedChannel.ChannelNumber);

            foreach(var channel in Channels)
            {
                if (channel.ChannelNumber > 2)
                    return;
                await channel.ChangeLoadModeAsync("PAR", state);

            }
        }

        [RelayCommand]
        private async Task SetSeriesModeAsync()
        {
            if (SelectedChannel == null || SelectedChannel.ChannelNumber > 2) return;

            if (Channels.Count <= 1)
                await AddChannel();

            bool state = await _deviceManager.SetSeriesModeAsync(SelectedChannel.ChannelNumber);
            foreach (var channel in Channels)
            {
                if (channel.ChannelNumber > 2)
                    return;
                await channel.ChangeLoadModeAsync("SER", state);

            }
        }
        #endregion

        #region Command for group channels
        public async Task SetIsetAsync(int channelNumber, double iset)
        {
            await _deviceManager.SetIsetAsync(channelNumber, iset);

        }
        public async Task SetVsetAsync(int channelNumber, double vset)
        {
            //var channel = Channels.First(c=>c.ChannelNumber == channelNumber);
            await _deviceManager.SetVsetAsync(channelNumber, vset);
        }

        public async Task SetCCLoadModeAsync(int channelNumber)
        {
            bool state = await _deviceManager.SetChannelLoadModeAsync(channelNumber, "CC");

            var channel = Channels.First(c=>c.ChannelNumber == channelNumber);
            channel.ChangeLoadMode("CC", state);
        }
        public async Task SetCVLoadModeAsync(int channelNumber)
        {
            bool state = await _deviceManager.SetChannelLoadModeAsync(channelNumber, "CV");

            var channel = Channels.First(c => c.ChannelNumber == channelNumber);
            channel.ChangeLoadMode("CV", state);
        }
        public async Task SetCRLoadModeAsync(int channelNumber)
        {
            bool state = await _deviceManager.SetChannelLoadModeAsync(channelNumber, "CR");

            var channel = Channels.First(c => c.ChannelNumber == channelNumber);
            channel.ChangeLoadMode("CR", state);
        }


        #endregion

        // Конструктор и инициализация
        #region Constructor and Initialization
        public DeviceViewModel(IDeviceService deviceManager, IPortDiscoverer portDiscoverer, IChannelPollingService channelPollingService)
        {
            _deviceManager = deviceManager;
            _portDiscoverer = portDiscoverer;
            _channelPollingService = channelPollingService;

            InitializeAsync();

            SubscribeToEvents();
        }

        private void SubscribeToEvents()
        {
            _deviceManager.NotificationReceived += OnDeviceNotification;
            _deviceManager.ErrorOccurred += OnDeviceError;
            _channelPollingService.MeasurementReceived += OnMeasurementReceived;
        }

        private async Task InitializeAsync()
        {
            try
            {
                var respones = await _deviceManager.GetDeviceInfoAsync();
                DeviceName = ParseDeviceName(respones);
            }
            catch (Exception ex)
            {
                // Handle exceptions appropriately
                // Maybe set a default value or error state
                DeviceName = "Unknown";
            }
        }


        private async Task InitializeChannels(int channel)
        {
            try
            {
                var initializedChannel = await _deviceManager.InitializeChannelsAsync(channel);
                var addedChannel = initializedChannel.newChannel;
                await addedChannel.ChangeLoadModeAsync(initializedChannel.LoadState);
                Channels.Add((PowerSupplyChannel)addedChannel);

                Channels.Last().DeviceName = DeviceName;
            }
            catch (Exception ex)
            {
                HandleError($"Ошибка инициализации каналов: {ex.Message}");
            }
        }
        #endregion

        // Обработчики событий
        #region Event Handlers
        private void OnMeasurementReceived(object? sender, ChannelMeasurementEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var channel = Channels.FirstOrDefault(c => c.ChannelNumber == e.ChannelNumber);
                if (channel != null)
                {
                    channel.Voltage = e.Measurements.Voltage;
                    channel.Current = e.Measurements.Current;
                    channel.Power = e.Measurements.Power;
                }
            });
        }

        private void OnDeviceNotification(object sender, DeviceNotificationEventArgs notification)
        {
            Application.Current.Dispatcher.Invoke(() => StatusMessage = notification.Message);
        }

        private void OnDeviceError(object sender, DeviceErrorEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() => HandleError(e.Error.Message));
        }
        #endregion

        // Вспомогательные методы
        #region Helper Methods
        private string ParseDeviceName(string deviceInfo)
        {
            var parts = deviceInfo.Split(',');
            return parts.Length > 1 ? parts[1].Trim() : "Unknown Device";
        }

        private void HandleError(string message)
        {
            StatusMessage = message;
            IsConnected = false;
        }
        #endregion

        // Очистка ресурсов
        #region IDisposable Implementation
        public void Dispose()
        {
            if (_disposed) return;

            CleanupCancellationTokens();
            UnsubscribeEvents();

            _deviceManager.Dispose();
            _channelPollingService.Dispose();
            _disposed = true;
        }

        private void CleanupCancellationTokens()
        {
            foreach (var cts in _cancellationTokens.Values)
            {
                cts.Cancel();
            }
            _cancellationTokens.Clear();
        }

        private void UnsubscribeEvents()
        {
            _deviceManager.NotificationReceived -= OnDeviceNotification;
            _deviceManager.ErrorOccurred -= OnDeviceError;
            _channelPollingService.MeasurementReceived -= OnMeasurementReceived;
        }
        #endregion

        #region Property

        public PowerSupplyChannel SelectedChannel
        {
            get => _selectedChannel;
            set
            {
                if (_selectedChannel != value)
                {
                    // Снимаем выделение с предыдущего канала
                    if (_selectedChannel != null)
                        _selectedChannel.IsSelected = false;

                    _selectedChannel = value;

                    // Устанавливаем выделение новому каналу
                    if (_selectedChannel != null)
                        _selectedChannel.IsSelected = true;

                    OnPropertyChanged();
                }
            }
        }
        #endregion
    }
}