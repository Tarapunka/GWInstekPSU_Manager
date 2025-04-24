using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace GWInstekPSUManager.Core.Interfaces.ChannelInterfaces;

public interface IPowerSupplyChannel : INotifyPropertyChanged, INotifyCollectionChanged, IDisposable
{
    string DeviceName { get; set; }
    int ChannelNumber { get; set; }
    double Voltage { get; set; }
    double Current { get; set; }
    double Power { get; set; }
    double Capacity { get; set; }
    double Vset { get; set; }
    double Iset { get; set; }
    double OVP { get; set; }
    double OCP { get; set; }

    double CurrentLimit { get; set; }
    double VoltageLimit { get; set; }

    double GroupActualVoltage  { get; set; }
    double GroupActualCurrent  { get; set; }
    double GroupVoltageLimit { get; set; }
    double GroupCurrentLimit { get; set; }

    string Mode { get; set; }
    DateTime StartTime { get; }
    TimeSpan ElapsedTime { get; }
    bool IsEnabled { get; set; }
    bool IsCalibrated { get; }
    bool IsLoadModeON { get; set; }
    bool IsCCModeOn { get; set; }
    bool IsCVModeOn { get; set; }
    bool IsCRModeOn { get; set; }
    bool IsParallelOn {  get; set; }
    bool IsSeriesOn { get; set; }
    bool IsSelected {  get; set; }

    Task ChangeLoadModeAsync(string mode, bool isEnabled = true);

    Task SaveToFileAsync(string filePath);
    Task LoadFromFileAsync(string filePath);
    Task CalibrateAsync();
    Task ResetAsync();
    IChannelCapacityCalculator CapacityCalculator { get; set; }
}