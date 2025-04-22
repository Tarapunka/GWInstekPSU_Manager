namespace GWInstekPSUManager.Core.Interfaces.ConnectionServices;

public interface IConnectionService : IDisposable
{
    bool IsConnected { get; }
    string ConnectionName { get; }
    Task ConnectAsync();
    Task DisconnectAsync();
    Task<string> SendQueryAsync(string query);
    Task SendCommandAsync(string command);
    Task ClearBuffersAsync();

    event EventHandler<string> DataReceived;
    event EventHandler<Exception> ErrorOccurred;
}
