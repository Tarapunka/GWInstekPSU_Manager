using GWInstekPSUManager.Core.Models;
using System;
using System.Text;

namespace GWInstekPSUManager.Core.Interfaces.DeviceInterfaces
{
    /// <summary>
    /// Интерфейс протокола общения с блоком питания GW Instek
    /// </summary>
    public interface IDeviceProtocol : IDisposable
    {
        /// <summary>
        /// Построитель команд для управления устройством
        /// </summary>
        ICommandBuilder Build { get; }

        /// <summary>
        /// Построитель запросов для получения данных
        /// </summary>
        IQueryBuilder Query { get; }

        /// <summary>
        /// Терминатор команд (окончание строки)
        /// </summary>
        string CommandTerminator { get; }

        /// <summary>
        /// Таймаут по умолчанию для операций
        /// </summary>
        TimeSpan DefaultTimeout { get; }

        /// <summary>
        /// Кодировка текста для обмена с устройством
        /// </summary>
        Encoding TextEncoding { get; }

        /// <summary>
        /// Парсинг ответа с дробными числами
        /// </summary>
        double ParseDoubleValueResponse(string response);

        /// <summary>
        /// Парсинг ответа c тремя данными
        /// </summary>
        MeasureResponse ParseMeasureResponse(string response);

        /// <summary>
        /// Парсинг строкового ответа
        /// </summary>

        string ParseStringResponse(string response);

        /// <summary>
        /// Попытка разбора уведомления от устройства
        /// </summary>
        bool TryParseNotification(string rawData, out DeviceNotification notification);

        /// <summary>
        /// Интерфейс построителя команд
        /// </summary>
        public interface ICommandBuilder
        {
            string EnableOutput(int channel, bool status);
            string SetVoltage(int channel, double voltage);
            string SetCurrent(int channel, double current);
            string SetOutput(int channel, bool enable);
            string SetOVPMode(int channel, bool enabled);
            string SetOVPValue(int channel, double voltage);
            string SetOCPMode(int channel, bool enabled);
            string SetOCPValue(int channel, double current);
            string SetLoadMode(int channel, string mode, bool enabled);
            string SetParallelMode(int channel, bool enabled);
            string SetSeriesMode(int channel, bool enabled);
            string Reset();
            string Beep();
        }

        /// <summary>
        /// Интерфейс построителя запросов
        /// </summary>
        public interface IQueryBuilder
        {
            string GetVoltage(int channel);
            string GetCurrent(int channel);
            string GetPower(int channel);
            string GetVsetValue(int channel);
            string GetIsetValue(int channel);
            string GetOutputState(int channel);
            string GetOVPMode(int channel);
            string GetOCPMode(int channel);
            string GetOVPValue(int channel);
            string GetOCPValue(int channel);
            string GetModeStatus(int channel);
            string GetMeasure(int channel);

            string GetDeviceInfo();
        }
    }
}