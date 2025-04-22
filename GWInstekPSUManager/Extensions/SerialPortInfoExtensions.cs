using GWInstekPSUManager.Core.Models;

namespace GWInstekPSUManager.Extensions;

public static class SerialPortInfoExtensions
{
    public static string GetDisplayName(this SerialPortInfo portInfo) => $"{portInfo.PortName} - {portInfo.Description} {(portInfo.IsGwInstekDevice ? "(GW Instek)" : "")}";
}