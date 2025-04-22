using GWInstekPSUManager.Core.Interfaces.ChannelInterfaces;
using GWInstekPSUManager.Core.Models;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Text.Json;

namespace GWInstekPSUManager.Infrastructure.Services.ChannelServices;

public partial class PowerSupplyChannel : IPowerSupplyChannel
{
    private double _voltage;
    private double _current;
    private double _capacity;
    private double _vset;
    private double _iset;
    private double _ovp;
    private double _ocp;
    private double _currentLimit;
    private double _voltageLimit;
    private string _mode = "CC";
    private DateTime _startTime = DateTime.Now;
    private bool _isEnabled = false;
    private bool _isCalibrated;
    private bool _disposed;

    private bool _isPollingActive = false;
    private bool _isLoadModeOn = false;
    private bool _isCVModeOn;
    private bool _isCCModeOn;
    private bool _isCRModeOn;
    private bool _isParallelOn;
    private bool _isSerriesOn;

    private bool _isSelected;


    private double _power;

    public event PropertyChangedEventHandler? PropertyChanged;
    public event NotifyCollectionChangedEventHandler? CollectionChanged;


    public int ChannelNumber { get; set; }

    public double Voltage
    {
        get => _voltage;
        set
        {
            //if (value < 0 || value > VoltageLimit)
            //    throw new ArgumentOutOfRangeException(nameof(value), $"Voltage must be between 0 and {VoltageLimit}V");

            if (SetField(ref _voltage, value, nameof(Voltage)))
                OnPropertyChanged(nameof(Voltage));
        }
    }

    public double Current
    {
        get => _current;
        set
        {
            //if (value < 0 || value > CurrentLimit)
            //    throw new ArgumentOutOfRangeException(nameof(value), $"Current must be between 0 and {CurrentLimit}A");

            if (SetField(ref _current, value, nameof(Current)))
                OnPropertyChanged(nameof(Current));
        }
    }

    public double Power
    {
        get => _power;
        set
        {
            if (SetField(ref _power, value, nameof(Power)))
                OnPropertyChanged(nameof(Power));
        }
    }


    public double Capacity
    {
        get => _capacity;
        set => SetField(ref _capacity, value, nameof(Capacity));
    }

    public double Vset
    {
        get => _vset;
        set
        {
            SetField(ref _vset, value, nameof(Vset));
            OnPropertyChanged(nameof(Vset));
        }
    }
    public double Iset
    {
        get => _iset;
        set
        {
            SetField(ref _iset, value, nameof(Iset));
            OnPropertyChanged(nameof(Iset));
        }
    }

    public double OVP
    {
        get => _ovp;
        set
        {
            SetField(ref _ovp, value, nameof(OVP));
            OnPropertyChanged(nameof(OVP));
        }
    }

    public double OCP
    {
        get => _ocp;
        set
        {
            SetField(ref _ocp, value, nameof(OCP));
            OnPropertyChanged(nameof(OCP));
        }
    }

    public double CurrentLimit
    {
        get => _currentLimit;
        set
        {
            if (value <= 0)
                throw new ArgumentOutOfRangeException(nameof(value), "Current limit must be positive");
            SetField(ref _currentLimit, value, nameof(CurrentLimit));
        }
    }

    public double VoltageLimit
    {
        get => _voltageLimit;
        set
        {
            if (value <= 0)
                throw new ArgumentOutOfRangeException(nameof(value), "Voltage limit must be positive");
            SetField(ref _voltageLimit, value, nameof(VoltageLimit));
        }
    }

    public string Mode
    {
        get => _mode;
        set
        {
            SetField(ref _mode, value, nameof(Mode));
        }
    }

    public DateTime StartTime
    {
        get => _startTime;
        set => SetField(ref _startTime, value, nameof(StartTime));
    }

    public TimeSpan ElapsedTime => DateTime.Now - StartTime;

    public bool IsEnabled
    {
        get => _isEnabled;
        set
        {
            SetField(ref _isEnabled, value, nameof(IsEnabled));
            OnPropertyChanged(nameof(IsEnabled));
        }
    }

    public bool IsCalibrated
    {
        get => _isCalibrated;
        set => SetField(ref _isCalibrated, value, nameof(IsCalibrated));
    }
    public bool IsPollingActive
    {
        get => _isPollingActive;
        set
        {
            SetField(ref _isPollingActive, value, nameof(IsPollingActive));
            OnPropertyChanged(nameof(IsPollingActive));
        }
    }
    public bool IsLoadModeON
    {
        get
        {
            if (IsCCModeOn || IsCVModeOn || IsCRModeOn)
                _isLoadModeOn = true;
            else
                _isLoadModeOn = false;
            return _isLoadModeOn;
        }

        set => SetField(ref _isLoadModeOn, value, nameof(IsLoadModeON));
    }

    public bool IsCCModeOn
    {
        get => _isCCModeOn;
        set
        {
            SetField(ref _isCCModeOn, value, nameof(IsCCModeOn));
            OnPropertyChanged(nameof(IsCCModeOn));
        }
    }


    public bool IsCVModeOn
    {
        get => _isCVModeOn;
        set
        {
            SetField(ref _isCVModeOn, value, nameof(IsCVModeOn));
            OnPropertyChanged(nameof(IsCVModeOn));

        }
    }
    public bool IsCRModeOn 
    {
        get => _isCRModeOn;
        set
        {
            SetField(ref _isCRModeOn, value, nameof(IsCRModeOn));
            OnPropertyChanged(nameof(IsCRModeOn));
        }
    }
    public bool IsParallelOn
    {
        get => _isParallelOn;
        set
        {
            SetField(ref _isParallelOn, value, nameof(IsParallelOn));
            OnPropertyChanged(nameof(IsParallelOn));
        }
    }
    public bool IsSeriesOn
    {
        get => _isSerriesOn;
        set
        {
            SetField(ref _isSerriesOn, value, nameof(IsSeriesOn));
            OnPropertyChanged(nameof(IsSeriesOn));
        }
    }

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            SetField(ref _isSelected, value, nameof(IsSelected));
            OnPropertyChanged(nameof(IsSelected));
        }
    }



    public async Task SaveToFileAsync(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be empty", nameof(filePath));

        var data = new
        {
            ChannelNumber,
            Voltage,
            Current,
            Capacity,
            CurrentLimit,
            VoltageLimit,
            Mode,
            StartTime,
            IsEnabled,
            IsCalibrated
        };

        var options = new JsonSerializerOptions { WriteIndented = true };
        await File.WriteAllTextAsync(filePath, JsonSerializer.Serialize(data, options));
    }

    public async Task LoadFromFileAsync(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("Channel data file not found", filePath);

        var json = await File.ReadAllTextAsync(filePath);
        var data = JsonSerializer.Deserialize<ChannelData>(json)
            ?? throw new InvalidDataException("Invalid channel data format");

        Voltage = data.Voltage;
        Current = data.Current;
        Capacity = data.Capacity;
        CurrentLimit = data.CurrentLimit;
        VoltageLimit = data.VoltageLimit;
        Mode = data.Mode;
        StartTime = data.StartTime;
        IsEnabled = data.IsEnabled;
        IsCalibrated = data.IsCalibrated;
    }

    public async Task CalibrateAsync()
    {
        // Эмуляция процесса калибровки
        await Task.Delay(1000);
        IsCalibrated = true;
        OnPropertyChanged(nameof(IsCalibrated));
    }

    public Task ResetAsync()
    {
        Voltage = 0;
        Current = 0;
        Capacity = 0;
        StartTime = DateTime.Now;
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        GC.SuppressFinalize(this);
    }

    private bool SetField<T>(ref T field, T value, string propertyName)
    {
        if (Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public async Task ChangeLoadModeAsync(string mode, bool isEnabled )
    {
        //// Фиктивный await для соответствия сигнатуре
        //await Task.CompletedTask;

        await Task.Run(() => ChangeLoadMode(mode, isEnabled));
    }

    public void ChangeLoadMode_old(string mode)
    {
        // Проверка входных данных
        if (string.IsNullOrEmpty(mode))
            throw new ArgumentException("Режим не может быть пустым");

        Mode = mode;

        // Простое переключение состояний
        switch (mode.ToUpper())
        {
            case "CC":
                IsCCModeOn = true;
                IsCVModeOn = false;
                IsCRModeOn = false;
                IsLoadModeON = true;
                break;

            case "CV":
                IsCCModeOn = false;
                IsCVModeOn = true;
                IsCRModeOn = false;
                IsLoadModeON = true;
                break;

            case "CR":
                IsCCModeOn = false;
                IsCVModeOn = false;
                IsCRModeOn = true;
                IsLoadModeON = true;
                break;

            case "IND":
                IsCCModeOn = false;
                IsCVModeOn = false;
                IsCRModeOn = false;
                IsLoadModeON = false;
                break;

            default:
                throw new ArgumentException($"Неподдерживаемый режим: {mode}");
        }
    }

    public void ChangeLoadMode(string mode, bool isEnabled = true)
    {
        // Проверка входных данных
        if (string.IsNullOrEmpty(mode))
            throw new ArgumentException("Режим не может быть пустым");

        Mode = "IND";

        // Сначала сбрасываем все режимы
        IsCCModeOn = false;
        IsCVModeOn = false;
        IsCRModeOn = false;
        IsLoadModeON = false;
        IsParallelOn = false;
        IsSeriesOn = false;

        // Если режим включен, устанавливаем соответствующий флаг
        if (isEnabled)
        {
            switch (mode.ToUpper())
            {
                case "CC":
                    Mode = mode;
                    IsCCModeOn = true;
                    IsLoadModeON = true;
                    break;

                case "CV":
                    Mode = mode;
                    IsCVModeOn = true;
                    IsLoadModeON = true;
                    break;

                case "CR":
                    Mode = mode;
                    IsCRModeOn = true;
                    IsLoadModeON = true;
                    break;

                case "IND":
                    // Для IND режима LoadModeON остается false
                    break;
                case "PAR":
                    Mode = mode;
                    IsParallelOn = true;
                    break;
                case "SER":
                    Mode = mode;
                    IsSeriesOn = true;
                    break;


                default:
                    throw new ArgumentException($"Неподдерживаемый режим: {mode}");
            }
        }
        // Если режим выключен (isEnabled = false), все флаги остаются false
    }
    public void UpdateChannelInfo(ChannelData channel )
    {
        Voltage = channel.Voltage;
        Current = channel.Current;
        Power = channel.Power;
        Capacity = channel.Capacity;
        StartTime = channel.StartTime;
        ChangeLoadMode_old(channel.Mode);
    }

}