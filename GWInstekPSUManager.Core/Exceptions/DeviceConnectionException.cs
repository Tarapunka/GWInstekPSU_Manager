namespace GWInstekPSUManager.Core.Exceptions;

public class DeviceConnectionException : Exception
{
    public DeviceConnectionException() { }
    public DeviceConnectionException(string message) : base(message) { }
    public DeviceConnectionException(string message, Exception inner)
        : base(message, inner) { }
}
