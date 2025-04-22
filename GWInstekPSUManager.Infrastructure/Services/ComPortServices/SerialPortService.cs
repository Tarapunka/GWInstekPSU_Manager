using GWInstekPSUManager.Core.Interfaces.ComPortsInterfaces;
using GWInstekPSUManager.Core.Models;
using System;
using System.Collections.Concurrent;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GWInstekPSUManager.Core.Interfaces.DeviceInterfaces;

public class SerialPortService : ISerialPortService
{
    private readonly string _portName;
    private readonly int _baudRate;
    private readonly Parity _parity;
    private readonly int _dataBits;
    private readonly StopBits _stopBits;
    private readonly TimeSpan _defaultTimeout;

    private SerialPort _serialPort;
    private Thread _backgroundThread;
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly ConcurrentQueue<string> _dataQueue = new();
    private bool _disposed;

    private readonly object _serialPortLock = new object();

    public bool IsOpen => _serialPort?.IsOpen ?? false;

    public TimeSpan Timeout
    {
        get => _defaultTimeout;
        set
        {
            if (_serialPort != null)
            {
                _serialPort.ReadTimeout = (int)value.TotalMilliseconds;
                _serialPort.WriteTimeout = (int)value.TotalMilliseconds;
            }
        }
    }

    public Encoding TextEncoding { get; set; } = Encoding.UTF8;

    public event EventHandler<string> DataReceived;

    public async Task OpenAsync(SerialPortSettings portSettings)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(SerialPortService));

        if (IsOpen)
            return; // Порт уже открыт

        try
        {
            _serialPort = new SerialPort
            {
                PortName = portSettings.PortName,
                BaudRate = portSettings.BaudRate,
                Parity = portSettings.Parity,
                DataBits = portSettings.DataBits,
                StopBits = portSettings.StopBits,
                Handshake = Handshake.None,
                //ReadTimeout = 1000000

            };

            await Task.Run(() => _serialPort.Open());

            _serialPort.DataReceived += OnDataReceivedFromSerialPort;

            // Запускаем фоновый поток для обработки данных
            _backgroundThread = new Thread(ProcessDataQueue) { IsBackground = true };
            _backgroundThread.Start();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to open serial port.", ex);
        }
    }

    public async Task CloseAsync()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(SerialPortService));
        }

        if (!IsOpen)
        {
            return; // Порт уже закрыт
        }

        try
        {
            _serialPort.DataReceived -= OnDataReceivedFromSerialPort;

            // Останавливаем фоновый поток
            _cancellationTokenSource.Cancel();
            _backgroundThread.Join();

            await Task.Run(() => _serialPort.Close());
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to close serial port.", ex);
        }
    }


    public async Task SendCommandAsync(string command)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(SerialPortService));

        if (!IsOpen)
            throw new InvalidOperationException("Serial port is not open.");

        try
        {
            await Task.Run(() => _serialPort.WriteLine(command));
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to send command through the serial port.", ex);
        }
    }

    public async Task<string> QueryAsync(string command)
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(SerialPortService));
        }

        if (!IsOpen)
        {
            throw new InvalidOperationException("Serial port is not open.");
        }

        try
        {
            await Task.Run(() => _serialPort.WriteLine(command));
            //var response = await Task.Run(() => _serialPort.ReadLine());

            if (!command.Contains('?'))
                return string.Empty;

            var response = await Task.Run(() => _serialPort.ReadExisting());
            if (response == string.Empty)
                response = await Task.Run(() => _serialPort.ReadLine());

            return response;
        }

        catch (TimeoutException)
        {
            throw new TimeoutException("The operation has timed out while waiting for a response from the device.");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to send or receive data through the serial port.", ex);
        }
    }

    private void OnDataReceivedFromSerialPort(object sender, SerialDataReceivedEventArgs e)
    {
        //try
        //{
        //    var serialPort = sender as SerialPort;
        //    if (serialPort == null) return;

        //    string data = serialPort.ReadExisting();

        //    // Помещаем данные в очередь
        //    _dataQueue.Enqueue(data);
        //}
        //catch (Exception ex)
        //{
        //    Console.Error.WriteLine($"Error in DataReceived handler: {ex.Message}");
        //}
    }

    private void ProcessDataQueue()
    {
        while (!_cancellationTokenSource.Token.IsCancellationRequested)
        {
            try
            {
                // Читаем данные из очереди
                if (_dataQueue.TryDequeue(out string data))
                {
                    // Передаем данные наверх через событие
                    DataReceived?.Invoke(this, data);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error processing data queue: {ex.Message}");
            }

            // Небольшая пауза, чтобы не перегружать CPU
            Thread.Sleep(10);
        }
    }

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
                if (IsOpen)
                {
                    _serialPort.DataReceived -= OnDataReceivedFromSerialPort;
                    _serialPort.Close();
                }

                _serialPort?.Dispose();
                _cancellationTokenSource.Cancel();
                _backgroundThread?.Join();
            }

            _disposed = true;
        }
    }

    ~SerialPortService()
    {
        Dispose(false);
    }
}