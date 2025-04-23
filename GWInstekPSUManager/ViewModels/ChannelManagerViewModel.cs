using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GWInstekPSUManager.Infrastructure.Services.ChannelServices;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace GWInstekPSUManager.ViewModels
{
    public partial class ChannelManagerViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<DeviceViewModel> _deviceViewModels;

        [ObservableProperty]
        private DeviceViewModel _selectedViewModel;

        [ObservableProperty]
        private PowerSupplyChannel _selectedChannel;

        [ObservableProperty]
        private ObservableCollection<PowerSupplyChannel> _availableChannels;

        [ObservableProperty]
        private ObservableCollection<PowerSupplyChannel> _selectedChannels;

        [ObservableProperty]
        private bool _isSeriesMode = false;

        [ObservableProperty]
        private bool _isParallelMode = false;




        public ChannelManagerViewModel(ObservableCollection<DeviceViewModel> deviceViewModels)
        {
            _deviceViewModels = deviceViewModels;
            _deviceViewModels.CollectionChanged += OnDevicesCollectionChanged;


            // Инициализируем AvailableChannels
            AvailableChannels = new ObservableCollection<PowerSupplyChannel>(
                _deviceViewModels.SelectMany(d => d.Channels)
            );

            SelectedChannels = new ObservableCollection<PowerSupplyChannel>();

            // Подписываемся на изменения каналов в каждом устройстве
            foreach (var device in _deviceViewModels)
            {
                device.Channels.CollectionChanged += OnChannelsCollectionChanged;
            }
        }

        private void OnDevicesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (DeviceViewModel device in e.NewItems)
                {
                    device.Channels.CollectionChanged += OnChannelsCollectionChanged;
                }
            }

            if (e.OldItems != null)
            {
                foreach (DeviceViewModel device in e.OldItems)
                {
                    device.Channels.CollectionChanged -= OnChannelsCollectionChanged;
                }
            }

            UpdateAvailableChannels();
        }

        private void OnChannelsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateAvailableChannels();
        }

        [RelayCommand]
        private void AddChannelToGroup() => SelectedChannels.Add(SelectedChannel);

        [RelayCommand]
        private void RemoveChannelFromGroup()
        {
            var channel = SelectedChannels.First(c => c == SelectedChannel);
            SelectedChannels.Remove(channel);
        }

        private void UpdateAvailableChannels()
        {
            var newChannels = _deviceViewModels
                .SelectMany(d => d.Channels)
                .ToList();

            // Синхронизируем AvailableChannels
            foreach (var channel in AvailableChannels.ToList())
            {
                if (!newChannels.Contains(channel))
                {
                    AvailableChannels.Remove(channel);
                }
            }

            foreach (var channel in newChannels)
            {
                if (!AvailableChannels.Contains(channel))
                {
                    AvailableChannels.Add(channel);
                }
            }
        }


        [RelayCommand]
        private async Task ToggleSelectedChannels()
        {
            foreach (var channel in SelectedChannels.ToList())
            {
                var device = _deviceViewModels.FirstOrDefault(d => d.Channels.Contains(channel));
                if (device != null)
                {
                    await device.TurnChannelAsync(channel.ChannelNumber);
                }
            }
        }

        [RelayCommand]
        private async Task SetVoltageForSelected(double voltage)
        {
            double volt = voltage;
            if (IsSeriesMode)
                volt = voltage / SelectedChannels.Count;

            foreach (var channel in SelectedChannels.ToList())
            {
                channel.Vset = volt;
                var device = _deviceViewModels.FirstOrDefault(d => d.Channels.Contains(channel));
                if (device != null)
                {
                    await device.SetVsetAsync(channel.ChannelNumber, volt);
                }
            }
        }

        [RelayCommand]
        private async Task SetCurrentForSelected(double current)
        {
            double cur = current;
            if (IsParallelMode)
                cur = current / SelectedChannels.Count;

            foreach (var channel in SelectedChannels.ToList())
            {
                channel.Iset = cur;
                var device = _deviceViewModels.FirstOrDefault(d => d.Channels.Contains(channel));
                if (device != null)
                {
                    await device.SetIsetAsync(channel.ChannelNumber, cur);
                }
            }
        }

        [RelayCommand]
        private async Task SetCCLoadMode()
        {
            foreach (var channel in SelectedChannels.ToList())
            {
                var device = _deviceViewModels.FirstOrDefault(d => d.Channels.Contains(channel));
                if (device != null)
                {
                  await device.SetCCLoadModeAsync(channel.ChannelNumber);
                }
            }
        }

        [RelayCommand]
        private async Task SetCVLoadMode()
        {
            foreach (var channel in SelectedChannels.ToList())
            {
                var device = _deviceViewModels.FirstOrDefault(d => d.Channels.Contains(channel));
                if (device != null)
                {
                    await device.SetCVLoadModeAsync(channel.ChannelNumber);
                }
            }
        }

        [RelayCommand]
        private async Task SetCRLoadMode()
        {
            foreach (var channel in SelectedChannels.ToList())
            {
                var device = _deviceViewModels.FirstOrDefault(d => d.Channels.Contains(channel));
                if (device != null)
                {
                    await device.SetCRLoadModeAsync(channel.ChannelNumber);
                }
            }
        }

        [RelayCommand]
        private async Task SetGroupCurrentLimit(double current)
        {
            foreach(var channel in SelectedChannels.ToList())
            {
                 await Task.Run(()=>channel.GroupCurrentLimit = current);
            }
        }

        [RelayCommand]
        private async Task SetGroupVoltageLimit(double voltage)
        {
            foreach (var channel in SelectedChannels.ToList())
            {
                await Task.Run(() => channel.GroupVoltageLimit = voltage);
            }
        }



    }
}