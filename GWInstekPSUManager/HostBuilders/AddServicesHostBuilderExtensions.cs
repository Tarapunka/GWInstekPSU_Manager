using GWInstekPSUManager.Core.Interfaces.ChannelInterfaces;
using GWInstekPSUManager.Core.Interfaces.ComPortsInterfaces;
using GWInstekPSUManager.Core.Interfaces.ConnectionServices;
using GWInstekPSUManager.Core.Interfaces.ConnectionServices.ComportConnectionServices;
using GWInstekPSUManager.Core.Interfaces.DeviceInterfaces;
using GWInstekPSUManager.Infrastructure.Services.ChannelServices;
using GWInstekPSUManager.Infrastructure.Services.ComPortServices;
using GWInstekPSUManager.Infrastructure.Services.ConectionServices;
using GWInstekPSUManager.Infrastructure.Services.DeviceServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GWInstekPSUManager.HostBuilders;

public static class AddServicesHostBuilderExtensions
{
    public static IHostBuilder AddAppServices(this IHostBuilder builder)
    {
        return builder.ConfigureServices(services =>
        {
            services.AddSingleton<IPortDiscoverer, SerialPortDiscoverer>();
            services.AddTransient<ISerialPortService, SerialPortService>();
            services.AddTransient<IDeviceConnection, DeviceConnectionService>();
            

            services.AddTransient<IDeviceService, DeviceFacadeService>();
            services.AddTransient<IDeviceProtocol,DeviceProtocol>();
            services.AddTransient<IDeviceSystem, DeviceSystemService>();
            
            services.AddTransient<IPowerSupplyChannel, PowerSupplyChannel>();
            services.AddTransient<IChannelController, ChannelControllerService>();
            services.AddTransient<IChannelPollingService, ChannelPollingService>();

            services.AddTransient<IConnectionService, ComPortService>();
            services.AddTransient<IConnectionServiceFactory<ComPortSettings>, ComPortServiceFactory>();
            services.AddTransient<IConnectionSettings,ComPortSettings>();
            services.AddTransient<IConnectionStrategy, ConnectionStrategy>();
            services.AddTransient<IComPortSettings, ComPortSettings>();


            services.AddTransient<ComPortSettings>();

        });
    }
}
