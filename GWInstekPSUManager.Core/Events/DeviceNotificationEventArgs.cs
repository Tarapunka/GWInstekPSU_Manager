namespace GWInstekPSUManager.Core.Events;

public class DeviceNotificationEventArgs : EventArgs
{
    public string Message { get; }
    public DateTime Timestamp { get; }
    public string RawData { get; }

    public DeviceNotificationEventArgs(string message, string rawData = null)
    {
        Message = message;
        RawData = rawData ?? message;
        Timestamp = DateTime.UtcNow;
    }
}