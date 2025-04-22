using System.IO.Ports;

namespace GWInstekPSUManager.Core.Interfaces.ComPortsInterfaces;

public interface ISerialPortServiceFactory
{    ISerialPortService Create();
}