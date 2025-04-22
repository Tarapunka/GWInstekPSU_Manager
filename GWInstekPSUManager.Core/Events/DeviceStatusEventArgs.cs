namespace GWInstekPSUManager.Core.Events;

public class DeviceStatusEventArgs : EventArgs
{
    public string StatusMessage { get; }
    public bool IsWarning { get; }
    public DateTime EventTime { get; }

    public DeviceStatusEventArgs(string message, bool isWarning = false)
    {
        StatusMessage = message;
        IsWarning = isWarning;
        EventTime = DateTime.Now;
    }
}