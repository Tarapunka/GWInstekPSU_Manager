using GWInstekPSUManager.Core.Interfaces.ChannelInterfaces;
using GWInstekPSUManager.Core.Interfaces.ConnectionServices;
using GWInstekPSUManager.Core.Interfaces.DeviceInterfaces;
using GWInstekPSUManager.Core.Models;

namespace GWInstekPSUManager.Infrastructure.Services.ChannelServices;

public class ChannelControllerService : IChannelController
{
    private readonly IConnectionService _connection;
    private readonly IDeviceProtocol _protocol;

    public ChannelControllerService(IConnectionService connection, IDeviceProtocol protocol)
    {
        _connection = connection;
        _protocol = protocol;
    }

    /// <summary>
    /// Инициализация канала.
    /// </summary>
    public async Task<(IPowerSupplyChannel newChannel, string LoadState)> InitializeChannelsAsync(int channel)
    {
        try
        {
            string state = "IND";
            var newChannel = new PowerSupplyChannel();
            var measure = await GetMeasureAsync(channel);
            var vset = await GetVsetValueAsync(channel);
            var iset = await GetIsetValueAsync(channel);
            if (channel < 3)
                state = await GetLoadModeAsync(channel);
            var isSwitchOn = await GetOutputState(channel);

            newChannel.ChannelNumber = channel;
            newChannel.Voltage = measure.Voltage;
            newChannel.Current = measure.Current;
            newChannel.Power = measure.Power;
            newChannel.IsEnabled = isSwitchOn;
            newChannel.Vset = vset;
            newChannel.Iset = iset;

            return (newChannel, state);
        }
        catch (Exception ex)
        {
            throw new Exception($"Ошибка инициализации каналов: {ex.Message}");
        }
    }


    /// <summary>
    /// Получение значений канала (напряжение, сила тока, мощность)
    /// </summary>
    public async Task<MeasureResponse> GetMeasureAsync(int channel)
    {
        var command = _protocol.Query.GetMeasure(channel);
        var response = await _connection.SendQueryAsync(command);
        return _protocol.ParseMeasureResponse(response);
    }


    /// <summary>
    /// Включение/выключение выхода канала
    /// </summary>
    public async Task<bool> SwitchEnableOutputAsync(int channel)
    {
        bool state = await GetOutputState(channel);
        var command = _protocol.Build.EnableOutput(channel, !state);
        await _connection.SendCommandAsync(command);
        return await GetOutputState(channel);
    }

    /// <summary>
    /// Получение на состояние канала (вкл/выкл)
    /// </summary>
    public async Task<bool> GetOutputState(int channel)
    {
        var command = _protocol.Query.GetOutputState(channel);
        return (await _connection.SendQueryAsync(command) == "ON");
    }


    /// <summary>
    /// Установка напряжения на канале
    /// </summary>
    public async Task SetVoltageAsync(int channel, double voltage)
    {
        var command = _protocol.Build.SetVoltage(channel, voltage);
        await _connection.SendCommandAsync(command);
    }

    /// <summary>
    /// Получение текущего напряжения
    /// </summary>
    public async Task<double> GetVoltageAsync(int channel)
    {
        var command = _protocol.Query.GetVoltage(channel);
        var response = await _connection.SendQueryAsync(command);
        return _protocol.ParseDoubleValueResponse(response);
    }

    /// <summary>
    /// Установка тока на канале
    /// </summary>
    public async Task SetCurrentAsync(int channel, double current)
    {
        var command = _protocol.Build.SetCurrent(channel, current);
        await _connection.SendCommandAsync(command);

    }

    /// <summary>
    /// Получение текущего тока
    /// </summary>
    public async Task<double> GetCurrentAsync(int channel)
    {
        var command = _protocol.Query.GetCurrent(channel);
        var response = await _connection.SendQueryAsync(command);
        return _protocol.ParseDoubleValueResponse(response);

    }

    /// <summary>
    /// Установка режима нагрузки
    /// </summary>
    public async Task<bool> SetChannelLoadModeAsync(int channel, string chanelMode)
    {
        var currentMode = await GetLoadModeAsync(channel);
        bool status = (currentMode != chanelMode);

        var command = _protocol.Build.SetLoadMode(channel, chanelMode, status);
        await _connection.SendCommandAsync(command);
        return (chanelMode == await GetLoadModeAsync(channel));
    }

    /// <summary>
    /// Запрос текущего режима нагрузки
    /// </summary>
    public async Task<string> GetLoadModeAsync(int channel)
    {
        var command = _protocol.Query.GetModeStatus(channel);
        return _protocol.ParseStringResponse(await _connection.SendQueryAsync(command));
    }


    /// <summary>
    /// Установка режима OVP
    /// </summary>
    public async Task<bool> SwitchOVPModeAsync(int channel)
    {
        bool status = await GetOVPStatusAsync(channel);
        var command = _protocol.Build.SetOVPMode(channel, !status);
        await _connection.SendCommandAsync(command);
        return await GetOVPStatusAsync(channel);
    }

    /// <summary>
    /// Установка значения OVP
    /// </summary>
    public async Task SetOVPValueAsync(int channel, double voltage)
    {
        var command = _protocol.Build.SetOVPValue(channel, voltage);
        await _connection.SendCommandAsync(command);
    }

    /// <summary>
    /// Запрос значения OVP
    /// </summary>
    public async Task<double> GetOVPValueAsync(int channel)
    {
        var command = _protocol.Query.GetOCPValue(channel);
        var respones = await _connection.SendQueryAsync(command);
        return _protocol.ParseDoubleValueResponse(respones);
    }

    /// <summary>
    /// Запрос текущего состояния OVP
    /// </summary>
    public async Task<bool> GetOVPStatusAsync(int channel)
    {
        var command = _protocol.Query.GetOVPMode(channel);
        return (await _connection.SendQueryAsync(command) == "ON");
    }


    /// <summary>
    /// Установка режима OCP
    /// </summary>
    public async Task<bool> SwitchOCPModeAsync(int channel)
    {
        bool status = await GetOCPStatusAsync(channel);

        var command = _protocol.Build.SetOCPMode(channel, !status);
        await _connection.SendCommandAsync(command);
        return await GetOCPStatusAsync(channel);
    }

    /// <summary>
    /// Установка значения OCP
    /// </summary>
    public async Task SetOCPValueAsync(int channel, double ocpValue)
    {
        var command = _protocol.Build.SetOCPValue(channel, ocpValue);
        await _connection.SendCommandAsync(command);

    }

    /// <summary>
    /// Запрос значения OCP
    /// </summary> 
    public async Task<double> GetOCPValueAsync(int channel)
    {
        var command = _protocol.Query.GetOCPValue(channel);
        var respones = await _connection.SendQueryAsync(command);
        return _protocol.ParseDoubleValueResponse(respones);
    }

    /// <summary>
    /// Запрос текущего состояния OCP
    /// </summary>
    public async Task<bool> GetOCPStatusAsync(int channel)
    {
        var command = _protocol.Query.GetOCPMode(channel);
        return (await _connection.SendQueryAsync(command) == "ON");
    }

    /// <summary>
    /// Запрос текущего Vset
    /// </summary>
    public async Task<double> GetVsetValueAsync(int channel)
    {
        var command = _protocol.Query.GetVsetValue(channel);
        return _protocol.ParseDoubleValueResponse( await _connection.SendQueryAsync(command));
    }

    /// <summary>
    /// Запрос текущего Iset
    /// </summary>
    public async Task<double> GetIsetValueAsync(int channel)
    {
        var command = _protocol.Query.GetIsetValue(channel);
        return _protocol.ParseDoubleValueResponse(await _connection.SendQueryAsync(command));
    }




    public void Dispose()
    {
        throw new NotImplementedException();
    }
}