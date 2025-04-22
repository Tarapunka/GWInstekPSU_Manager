using GWInstekPSUManager.Core.Models;
using System.Data.Common;

namespace GWInstekPSUManager.Core.Interfaces.ChannelInterfaces;

// <summary>
/// Интерфейс для управления отдельными каналами блока питания
/// </summary>
public interface IChannelController: IDisposable
{
    /// <summary>
    /// Инициализация канала.
    /// </summary>
    public Task<(IPowerSupplyChannel newChannel, string LoadState)> InitializeChannelsAsync(int channel);

    /// <summary>
    /// Включение/выключение выхода канала
    /// </summary>
    Task<bool> SwitchEnableOutputAsync(int channel);

    /// <summary>
    /// Получение на состояние канала (вкл/выкл)
    /// </summary>
    Task<bool> GetOutputState(int channel);

    /// <summary>
    /// Получение значений канала (напряжение, сила тока, мощность)
    /// </summary>
    Task<MeasureResponse> GetMeasureAsync(int channel);

    /// <summary>
    /// Установка напряжения на канале
    /// </summary>
    Task SetVoltageAsync(int channel, double voltage);

    /// <summary>
    /// Получение текущего напряжения
    /// </summary>
    Task<double> GetVoltageAsync(int channel);

    /// <summary>
    /// Запрос текущего Vset
    /// </summary>
    Task<double> GetVsetValueAsync(int channel);


    /// <summary>
    /// Установка тока на канале
    /// </summary>
    Task SetCurrentAsync(int channel, double current);

    /// <summary>
    /// Получение текущего тока
    /// </summary>
    Task<double> GetCurrentAsync(int channel);

    /// <summary>
    /// Запрос текущего Iset
    /// </summary>
    Task<double> GetIsetValueAsync(int channel);


    /// <summary>
    /// Установка режима нагрузки
    /// </summary>
    Task<bool> SetChannelLoadModeAsync(int channel, string chanelMode);

    /// <summary>
    /// Запрос текущего режима нагрузки
    /// </summary>
    Task<string> GetLoadModeAsync(int channel);


    /// <summary>
    /// Установка режима OVP
    /// </summary>
    Task<bool> SwitchOVPModeAsync(int channel);

    /// <summary>
    /// Установка значения OVP
    /// </summary>
    Task SetOVPValueAsync(int channel, double voltage);

    /// <summary>
    /// Запрос значения OVP
    /// </summary>
    Task<double> GetOVPValueAsync(int channel);

    /// <summary>
    /// Запрос текущего состояния OVP
    /// </summary>
    Task<bool> GetOVPStatusAsync(int channel);


    /// <summary>
    /// Установка режима OCP
    /// </summary>
    Task<bool> SwitchOCPModeAsync(int channel);

    /// <summary>
    /// Установка значения OCP
    /// </summary>
    Task SetOCPValueAsync(int channel, double ocpValue);

    /// <summary>
    /// Запрос значения OCP
    /// </summary> 
    Task<double> GetOCPValueAsync(int channel);

    /// <summary>
    /// Запрос текущего состояния OCP
    /// </summary>
    Task<bool> GetOCPStatusAsync(int channel);


}