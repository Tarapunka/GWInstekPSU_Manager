using System.IO.Ports;

namespace GWInstekPSUManager.Core.Interfaces.ConnectionServices.ComportConnectionServices;

public interface IComPortSettings : IConnectionSettings
{
    string PortName { get; set; }
    int BaudRate { get; set; }
    Parity Parity { get; set; }
    int DataBits { get; set; }
    StopBits StopBits { get; set; }
    int ReadTimeout { get; set; }
    int WriteTimeout { get; set; }
    TimeSpan Timeout { get; set; }
}