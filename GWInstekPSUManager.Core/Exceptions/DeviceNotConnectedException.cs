namespace GWInstekPSUManager.Core.Exceptions;

public class DeviceNotConnectedException : DeviceConnectionException
{
    public DeviceNotConnectedException()
        : base("Device is not connected. Please connect first.") { }
}