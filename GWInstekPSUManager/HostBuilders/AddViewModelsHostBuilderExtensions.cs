using GWInstekPSUManager.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GWInstekPSUManager.HostBuilders;

public static class AddViewModelsHostBuilderExtensions
{
    public static IHostBuilder AddViewModels(this IHostBuilder builder)
    {
        return builder.ConfigureServices(services =>
        {
            services.AddTransient<MainViewModel>();
            services.AddTransient<DeviceViewModel>();

        });
    }
}