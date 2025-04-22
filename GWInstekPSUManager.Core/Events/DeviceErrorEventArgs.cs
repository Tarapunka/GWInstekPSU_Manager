namespace GWInstekPSUManager.Core.Events;

public class DeviceErrorEventArgs : EventArgs
{
    public Exception Error { get; }
    public string Command { get; }
    public bool IsCritical { get; }

    public DeviceErrorEventArgs(Exception error, string command = null, bool isCritical = true)
    {
        Error = error;
        Command = command;
        IsCritical = isCritical;
    }
}