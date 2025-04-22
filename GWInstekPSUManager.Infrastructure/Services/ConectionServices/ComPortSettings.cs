using GWInstekPSUManager.Core.Interfaces.ConnectionServices.ComportConnectionServices;
using System.IO.Ports;

namespace GWInstekPSUManager.Infrastructure.Services.ConectionServices;

public class ComPortSettings : IComPortSettings
{
    public string PortName { get; set ; }
    public int BaudRate { get; set; }
    public Parity Parity { get; set ; } = Parity.None;
    public int DataBits { get; set; }
    public StopBits StopBits { get; set; } = StopBits.None;
    public int ReadTimeout { get; set; } = -1;
    public int WriteTimeout { get; set; } = 500;
    public TimeSpan Timeout { get; set; } = TimeSpan.Zero;
    public int TimeoutMs { get; set; } = 1000;
}
