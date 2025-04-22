using GWInstekPSUManager.Core.Models;
using System.IO.Ports;
using System.Text;

namespace GWInstekPSUManager.Core.Interfaces.ComPortsInterfaces;

public interface ISerialPortService : IDisposable
{
        /// <summary>
    /// Отправляет команду через последовательный порт и возвращает ответ устройства.
    /// </summary>
    /// <param name="command">Команда для отправки.</param>
    /// <returns>Ответ устройства в виде строки.</returns>
    Task<string> QueryAsync(string command);

    /// <summary>
    /// Отправляет команду через последовательный порт без ответа.
    /// </summary>
    Task SendCommandAsync(string command);

    /// <summary>
    /// Асинхронно открывает соединение с последовательным портом.
    /// </summary>
    Task OpenAsync(SerialPortSettings portSettings);

    /// <summary>
    /// Асинхронно закрывает соединение с последовательным портом.
    /// </summary>
    Task CloseAsync();

    /// <summary>
    /// Проверяет, открыто ли соединение с последовательным портом.
    /// </summary>
    bool IsOpen { get; }

    /// <summary>
    /// Таймаут для операций чтения/записи.
    /// </summary>
    TimeSpan Timeout { get; set; }

    /// <summary>
    /// Кодировка текста для обмена данными с устройством.
    /// </summary>
    Encoding TextEncoding { get; set; }

    event EventHandler<string> DataReceived;
}