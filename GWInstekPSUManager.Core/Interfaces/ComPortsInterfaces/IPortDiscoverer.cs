using GWInstekPSUManager.Core.Models;

namespace GWInstekPSUManager.Core.Interfaces.ComPortsInterfaces;

public interface IPortDiscoverer
{
    /// <summary> Возвращает все доступные COM-порты </summary>
    IEnumerable<SerialPortInfo> GetAvailablePorts();

    /// <summary> Проверяет, является ли порт устройством GW Instek </summary>
    bool IsGwInstekPort(string portName);
}