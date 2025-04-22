using System.IO.Ports;

namespace GWInstekPSUManager.Core.Models;

public class SerialPortSettings
{
    public string PortName { get; set; }
    public int BaudRate { get; set; }
    public Parity Parity { get; set; }
    public int DataBits { get; set; }
    public StopBits StopBits { get; set; }
    public int ReadTimeout { get; set; }
    public int WriteTimeout { get; set; }
    public TimeSpan Timeout { get; set; }
}
