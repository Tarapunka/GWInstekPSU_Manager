using GWInstekPSUManager.Core.Interfaces.DeviceInterfaces;
using GWInstekPSUManager.Core.Interfaces;
using GWInstekPSUManager.Infrastructure.Services.ComPortServices;
using GWInstekPSUManager.Infrastructure.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using GWInstekPSUManager.Infrastructure.Services.DeviceServices;
using Microsoft.Extensions.DependencyInjection;

namespace GWInstekPSUManager.HostBuilders;

public static class AddLoggersHostBuilderExtensions
{
    public static IHostBuilder AddAppLoggers(this IHostBuilder builder)
    {
        return builder.ConfigureServices(services =>
        {
            services.AddLogging(configure => configure.AddDebug().AddConsole());

            //services.AddSingleton<ILogger<SerialPortTransport>>(provider =>
            //    provider.GetRequiredService<ILoggerFactory>().CreateLogger<SerialPortTransport>());
            //services.AddSingleton<ILogger<GWInstekProtocol>>(provider =>
            //    provider.GetRequiredService<ILoggerFactory>().CreateLogger<GWInstekProtocol>());
            //services.AddSingleton<ILogger<DeviceManager>>(provider =>
            //    provider.GetRequiredService<ILoggerFactory>().CreateLogger<DeviceManager>());
        });
    }
}
