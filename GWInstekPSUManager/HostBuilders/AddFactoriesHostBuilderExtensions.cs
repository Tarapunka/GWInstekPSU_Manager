using GWInstekPSUManager.Core.Interfaces.ChannelInterfaces;
using GWInstekPSUManager.Core.Interfaces.ComPortsInterfaces;
using GWInstekPSUManager.Core.Interfaces.DeviceInterfaces;
using GWInstekPSUManager.Infrastructure.Services;
using GWInstekPSUManager.Infrastructure.Services.ChannelServices;
using GWInstekPSUManager.Infrastructure.Services.ComPortServices;
using GWInstekPSUManager.Infrastructure.Services.DeviceServices.DeviceFactories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GWInstekPSUManager.HostBuilders;

public static class AddFactoriesHostBuilderExtensions
{
    public static IHostBuilder AddAppFactories(this IHostBuilder builder)
    {
        return builder.ConfigureServices(services =>
        {
            services.AddSingleton<ISerialPortServiceFactory,SerialPortServiceFactory>();

            services.AddSingleton<IDeviceProtocolFactory, DeviceProtocolFactory>();
            services.AddSingleton<IDeviceServiceFactory, DeviceServiceFactory>();
            services.AddTransient<IDeviceServiceFactory, DeviceServiceFactory>();

            //services.AddTransient<IDeviceConnectionFactory, DeviceConnectionFactory>();

            services.AddTransient<IChannelControllerFactory, ChannelControllerFactory>();

            services.AddSingleton<IChannelFactory, ChannelFactory>();

        });
    }

}
