using GWInstekPSUManager.Core.Interfaces.ComPortsInterfaces;
using GWInstekPSUManager.Core.Models;
using System.IO.Ports;
using System.Management;
using System.Text.RegularExpressions;

namespace GWInstekPSUManager.Infrastructure.Services.ComPortServices;

public class SerialPortDiscoverer : IPortDiscoverer
{
    public IEnumerable<SerialPortInfo> GetAvailablePorts()
    {
        var ports = new List<SerialPortInfo>();

        // 1. Получаем порты через WMI (детальная информация)
        try
        {
            using var searcher = new ManagementObjectSearcher(
                "SELECT * FROM Win32_PnPEntity WHERE Caption LIKE '%(COM%'");

            foreach (ManagementObject device in searcher.Get())
            {
                var portInfo = ParseDeviceInfo(device);
                if (portInfo != null) ports.Add(portInfo);
            }
        }
        catch { /* WMI может не работать в некоторых окружениях */ }

        // 2. Добавляем порты, которые не определились через WMI
        foreach (var portName in SerialPort.GetPortNames())
        {
            if (ports.Exists(p => p.PortName == portName)) continue;

            ports.Add(new SerialPortInfo
            {
                PortName = portName,
                Description = "Unknown port",
                IsGwInstekDevice = false
            });
        }

        return ports;
    }

    public bool IsGwInstekPort(string portName)
    {
        var ports = GetAvailablePorts();
        return ports.Any(p => p.PortName == portName && p.IsGwInstekDevice);
    }

    private SerialPortInfo? ParseDeviceInfo(ManagementObject device)
    {
        try
        {
            string caption = device["Caption"]?.ToString() ?? "";
            var match = Regex.Match(caption, @"\((COM\d+)\)");
            if (!match.Success) return null;

            return new SerialPortInfo
            {
                PortName = match.Groups[1].Value,
                Description = caption,
                IsGwInstekDevice = caption.Contains("GW Instek") ||
                                 caption.Contains("GPP") ||
                                 caption.Contains("PS-")
            };
        }
        catch
        {
            return null;
        }
    }
}