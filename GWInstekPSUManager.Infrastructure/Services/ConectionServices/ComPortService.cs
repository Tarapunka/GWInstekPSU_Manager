using GWInstekPSUManager.Core.Interfaces.ConnectionServices;
using GWInstekPSUManager.Core.Interfaces.ConnectionServices.ComportConnectionServices;
using System.IO.Ports;

namespace GWInstekPSUManager.Infrastructure.Services.ConectionServices;

public class ComPortService : IConnectionService
{
    private readonly SerialPort _serialPort;
    private readonly IComPortSettings _connectionSettings;
    private bool _disposed;
    private string _connectionName;
    private readonly SemaphoreSlim _serialSemaphore = new SemaphoreSlim(1, 1);

    public ComPortService(IComPortSettings connectionSettings)
    {
        _serialPort = new SerialPort();
        _connectionName = connectionSettings.PortName;
        _connectionSettings = connectionSettings ?? throw new ArgumentNullException(nameof(connectionSettings));
    }

    public bool IsConnected => _serialPort.IsOpen;
    public string ConnectionName => _connectionName;
    public event EventHandler<string> DataReceived;
    public event EventHandler<Exception> ErrorOccurred;

    public async Task ConnectAsync()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(ComPortService));

        if (IsConnected)
            return;

        try
        {
            ConfigureSerialPort();
            await Task.Run(() => _serialPort.Open());
            _serialPort.DataReceived += OnDataReceivedFromSerialPort;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to open serial port.", ex);
        }
    }

    private void ConfigureSerialPort()
    {
        _serialPort.PortName = _connectionSettings.PortName;
        _serialPort.BaudRate = _connectionSettings.BaudRate;
        _serialPort.Parity = _connectionSettings.Parity;
        _serialPort.DataBits = _connectionSettings.DataBits;
        _serialPort.StopBits = _connectionSettings.StopBits;
        _serialPort.ReadTimeout = _connectionSettings.ReadTimeout;
        _serialPort.WriteTimeout = _connectionSettings.WriteTimeout;
    }

    private void OnDataReceivedFromSerialPort(object sender, SerialDataReceivedEventArgs e)
    {
        //try
        //{
        //    var data = _serialPort.ReadExisting();
        //    DataReceived?.Invoke(this, data);
        //}
        //catch (Exception ex)
        //{
        //    ErrorOccurred?.Invoke(this, ex);
        //}
    }

    public async Task DisconnectAsync()
    {
        if (_disposed || !IsConnected)
            return;

        try
        {
            _serialPort.DataReceived -= OnDataReceivedFromSerialPort;
            await Task.Run(() => _serialPort.Close());
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to close serial port.", ex);
        }
    }

    public async Task SendCommandAsync(string command)
    {
        if (_disposed) return;
        if (!IsConnected) throw new InvalidOperationException("Serial port is not open.");

        await _serialSemaphore.WaitAsync();
        try
        {
            _serialPort.WriteLine(command);
        }
        finally
        {
            _serialSemaphore.Release();
        }
    }

    public async Task<string> SendQueryAsync(string query)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(ComPortService));
        if (!IsConnected) throw new InvalidOperationException("Serial port is not open.");

        await _serialSemaphore.WaitAsync();
        try
        {
            _serialPort.WriteLine(query);

            if (!query.Contains('?')) return string.Empty;

            string response = _serialPort.ReadExisting();
            if (string.IsNullOrEmpty(response))
            {
                response = _serialPort.ReadLine();
            }
            return response;
        }
        catch (TimeoutException)
        {
            throw new TimeoutException("Operation timed out while waiting for response.");
        }
        finally
        {
            _serialSemaphore.Release();
        }
    }

    public Task ClearBuffersAsync()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(ComPortService));

        return Task.Run(() =>
        {
            _serialPort.DiscardInBuffer();
            _serialPort.DiscardOutBuffer();
        });
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
                if (IsConnected)
                {
                    _serialPort.DataReceived -= OnDataReceivedFromSerialPort;
                    _serialPort.Close();
                }
                _serialPort?.Dispose();
            }
            _disposed = true;
        }
    }

    ~ComPortService() => Dispose(false);
}