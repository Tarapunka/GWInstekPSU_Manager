namespace GWInstekPSUManager.Core.Models;

public enum NotificationType
{
    StatusUpdate,
    OverVoltage,
    OverCurrent,
    OverTemperature,
    DeviceError
}

public record DeviceNotification
{
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public NotificationType Type { get; init; }
    public int? Channel { get; init; }
    public string Message { get; init; }
    public string RawData { get; init; }

    public static DeviceNotification CreateError(string rawData, string message = null)
    {
        return new DeviceNotification
        {
            Type = NotificationType.DeviceError,
            Message = message ?? "Device error occurred",
            RawData = rawData
        };
    }
}