using GWInstekPSUManager.Core.Interfaces.DeviceInterfaces;
using GWInstekPSUManager.Core.Models;
using System.Globalization;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Channels;
using static GWInstekPSUManager.Core.Interfaces.DeviceInterfaces.IDeviceProtocol;

namespace GWInstekPSUManager.Infrastructure.Services.DeviceServices;

public class DeviceProtocol : IDeviceProtocol, IDisposable
{
    private bool _disposed = false;

    #region Базовые параметры протокола
    /// <summary>
    /// Терминатор команд (окончание строки)
    /// </summary>
    public string CommandTerminator => "\r\n";

    /// <summary>
    /// Таймаут по умолчанию для операций
    /// </summary>
    public TimeSpan DefaultTimeout => TimeSpan.FromSeconds(5);

    /// <summary>
    /// Кодировка текста для обмена с устройством
    /// </summary>
    public Encoding TextEncoding => Encoding.ASCII;
    #endregion

    #region Построители команд (Command Builders)
    /// <summary>
    /// Построитель команд для изменения параметров
    /// </summary>
    public ICommandBuilder Build => new CommandBuilder();

    /// <summary>
    /// Построитель запросов для получения данных
    /// </summary>
    public IQueryBuilder Query => new QueryBuilder();

    /// <summary>
    /// Класс для построения команд управления
    /// </summary>
    public class CommandBuilder : ICommandBuilder
    {
        public string EnableOutput(int channel, bool status)
        {
            ValidateChannelNumber(channel);
            return $":OUTPut{channel} {(status ? "ON" : "OFF")}";
        }

        /// <summary>
        /// Установка напряжения на канале
        /// </summary>
        public string SetVoltage(int channel, double voltage)
        {
            ValidateChannelNumber(channel);
            return $":SOURce{channel}:VOLTage {FormatNumericValue(voltage)}";
        }

        /// <summary>
        /// Установка тока на канале
        /// </summary>
        public string SetCurrent(int channel, double current)
        {
            ValidateChannelNumber(channel);
            return $":SOURce{channel}:CURRent {FormatNumericValue(current)}";
        }

        /// <summary>
        /// Включение/выключение выхода канала
        /// </summary>
        public string SetOutput(int channel, bool enable)
        {
            ValidateChannelNumber(channel);
            return $":OUTPut{channel} {(enable ? "ON" : "OFF")}";
        }

        /// <summary>
        /// Установка режима OVP
        /// </summary>
        public string SetOVPMode(int channel, bool enabled)
        {
            ValidateChannelNumber(channel);
            return $":OUTPut{channel}:OVP:STATe {(enabled ? "ON" : "OFF")}";
        }

        /// <summary>
        /// Установка значения OVP
        /// </summary>
        public string SetOVPValue(int channel, double voltage)
        {
            ValidateChannelNumber(channel);
            return $":OUTPut{channel}:OVP {FormatNumericValue(voltage)}";
        }

        /// <summary>
        /// Установка режима OCP
        /// </summary>
        public string SetOCPMode(int channel, bool enabled)
        {
            ValidateChannelNumber(channel);
            return $":OUTPut{channel}:OCP:STATe {(enabled ? "ON" : "OFF")}";
        }

        /// <summary>
        /// Установка значения OCP
        /// </summary>
        public string SetOCPValue(int channel, double current)
        {
            ValidateChannelNumber(channel);
            return $":OUTPut{channel}:OCP {FormatNumericValue(current)}";
        }

        /// <summary>
        /// Установка режима нагрузки
        /// </summary>
        public string SetLoadMode(int channel, string mode, bool enabled)
        {
            ValidateChannelNumber(channel);
            return $":LOAD{channel}:{mode} {(enabled ? "ON" : "OFF")}";
        }

        public string SetParallelMode(int channel, bool enabled)
        {
            ValidateChannelNumber(channel);
            return $":OUTPut:PARallel {(enabled ? "ON" : "OFF")}";
        }

        public string SetSeriesMode(int channel, bool enabled)
        {
            ValidateChannelNumber(channel);
            return $":OUTPut:SERies {(enabled ? "ON" : "OFF")}";

        }



        /// <summary>
        /// Сброс устройства
        /// </summary>
        public string Reset() => "*RST";

        /// <summary>
        /// Генерация звукового сигнала
        /// </summary>
        public string Beep() => ":SYSTem:BEEPer:IMMediate";

        private static string FormatNumericValue(double value)
        {
            return value.ToString("0.####", CultureInfo.InvariantCulture);
        }

        private static void ValidateChannelNumber(int channel)
        {
            if (channel < 1 || channel > 4)
                throw new ArgumentOutOfRangeException(nameof(channel), "Channel number must be between 1 and 4");
        }
    }

    /// <summary>
    /// Класс для построения запросов данных
    /// </summary>
    public class QueryBuilder : IQueryBuilder
    {
        /// <summary>
        /// Запрос напряжения на канале
        /// </summary>
        public string GetVoltage(int channel)
        {
            ValidateChannelNumber(channel);
            return $":MEASure{channel}:VOLTage?";
        }

        /// <summary>
        /// Запрос тока на канале
        /// </summary>
        public string GetCurrent(int channel)
        {
            ValidateChannelNumber(channel);
            return $":MEASure{channel}:CURRent?";
        }

        /// <summary>
        /// Запрос мощности на канале
        /// </summary>
        public string GetPower(int channel)
        {
            ValidateChannelNumber(channel);
            return $":MEASure{channel}:POWer?";
        }

        /// <summary>
        /// Запрос Vset
        /// </summary>
        public string GetVsetValue(int channel)
        {
            ValidateChannelNumber(channel);
            return $":SOURce{channel}:VOLTage?";
        }

        /// <summary>
        /// Запрос Iset
        /// </summary>
        public string GetIsetValue(int channel)
        {
            ValidateChannelNumber(channel);
            return $":SOURce{channel}:CURRent?";
        }



        /// <summary>
        /// Запрос состояния выхода канала
        /// </summary>
        public string GetOutputState(int channel)
        {
            ValidateChannelNumber(channel);
            return $":OUTPut{channel}:STATe?";
        }

        /// <summary>
        /// Запрос информации об устройстве
        /// </summary>
        public string GetDeviceInfo() => "*IDN?";

        private static void ValidateChannelNumber(int channel)
        {
            if (channel < 1 || channel > 4)
                throw new ArgumentOutOfRangeException(nameof(channel), "Channel number must be between 1 and 4");
        }

        public string GetOVPMode(int channel)
        {
            ValidateChannelNumber(channel);
            return $":OUTPut{channel}:OVP:STATe?";
        }

        public string GetOCPMode(int channel)
        {
            ValidateChannelNumber(channel);
            return $":OUTPut{channel}:OCP:STATe?";
        }
        public string GetOVPValue(int channel)
        {
            ValidateChannelNumber(channel);
            return $":OUTPut{channel}:OVP?";
        }

        public string GetOCPValue(int channel)
        {
            ValidateChannelNumber(channel);
            return $":OUTPut{channel}:OCP?";
        }

        public string GetModeStatus(int channel)
        {
            ValidateChannelNumber(channel);
            return $":MODE{channel}?";
        }

        public string GetMeasure(int channel)
        {
            ValidateChannelNumber(channel);
            return $":MEASure{channel}:ALL?";
        }



    }
    #endregion

    #region Парсинг ответов
    /// <summary>
    /// Парсинг значения напряжения из ответа устройства
    /// </summary>
    public double ParseDoubleValueResponse(string response)
    {
        if (TryParseNumericResponse(response, out double value))
        {
            return value;
        }
        throw new FormatException($"Invalid voltage response format: {response}");
    }

    public string ParseStringResponse(string response)
    {
        return response.TrimEnd();
    }

    /// <summary>
    /// Парсинг значения тока из ответа устройства
    /// </summary>
    public MeasureResponse ParseMeasureResponse(string response)
    {
        try
        {
            var responseArray = response.Split(",");
            if (responseArray.Length == 3)
            {
                return new MeasureResponse
                {
                    Voltage = ParseDoubleValueResponse(responseArray[0]),
                    Current = ParseDoubleValueResponse(responseArray[1]),
                    Power = ParseDoubleValueResponse(responseArray[2]),
                };
            }
            return new MeasureResponse
            {
                Voltage = 0,
                Current = 0,
                Power = 0,
            };
        }
        catch
        {
            return new MeasureResponse
            {
                Voltage = 0,
                Current = 0,
                Power = 0,
            };
            throw new FormatException($"Invalid Measure response format: {response}");
        }
    }

    /// <summary>
    /// Попытка парсинга уведомления от устройства
    /// </summary>
    public bool TryParseNotification(string rawData, out DeviceNotification notification)
    {
        notification = null;

        if (string.IsNullOrWhiteSpace(rawData))
            return false;

        try
        {
            // Пример обработки ошибки
            if (rawData.StartsWith("ERR:"))
            {
                notification = new DeviceNotification
                {
                    Type = NotificationType.DeviceError,
                    Message = ParseErrorMessage(rawData),
                    RawData = rawData,
                    Timestamp = DateTime.UtcNow
                };
                return true;
            }

            // Пример обработки предупреждения
            if (rawData.StartsWith("WARN:"))
            {
                notification = new DeviceNotification
                {
                    Type = NotificationType.StatusUpdate,
                    Message = ParseWarningMessage(rawData),
                    RawData = rawData,
                    Timestamp = DateTime.UtcNow
                };
                return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    private string ParseErrorMessage(string rawData)
    {
        // Реальная реализация зависит от формата ошибок вашего устройства
        var parts = rawData.Split(':');
        return parts.Length > 1 ? $"Device error: {parts[1]}" : "Unknown device error";
    }

    private string ParseWarningMessage(string rawData)
    {
        var parts = rawData.Split(':');
        return parts.Length > 1 ? $"Device warning: {parts[1]}" : "Unknown device warning";
    }

    private bool TryParseNumericResponse(string response, out double value)
    {
        value = 0;
        if (string.IsNullOrWhiteSpace(response))
            return false;

        return double.TryParse(
            response.Trim(),
            NumberStyles.Any,
            CultureInfo.InvariantCulture,
            out value
        );
    }
    #endregion

    #region Реализация IDisposable
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // Освобождение управляемых ресурсов
            }

            // Освобождение неуправляемых ресурсов
            _disposed = true;
        }
    }

    ~DeviceProtocol()
    {
        Dispose(false);
    }
    #endregion
}