namespace GWInstekPSUManager.Core.Exceptions;

public class DeviceOperationException : Exception
{
    public string Command { get; }
    public string Response { get; }

    public DeviceOperationException(string command, string response, string message)
        : base($"Command '{command}' failed. Response: '{response}'. {message}")
    {
        Command = command;
        Response = response;
    }
}