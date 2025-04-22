namespace GWInstekPSUManager.Core.Interfaces.ConnectionServices;

public interface IConnectionStrategy
{
    IConnectionService CreateConnectionService(ConnectionType type, IConnectionSettings connectionSettings);
}

public enum ConnectionType
{
    ComPort,
    // Можно добавить другие типы подключений
}