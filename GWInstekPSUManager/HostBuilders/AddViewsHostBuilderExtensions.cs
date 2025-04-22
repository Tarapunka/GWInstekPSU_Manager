// Extensions/AddViewsHostBuilderExtensions.cs
using GWInstekPSUManager.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GWInstekPSUManager.HostBuilders;

public static class AddViewsHostBuilderExtensions
{
    public static IHostBuilder AddViews(this IHostBuilder builder)
    {
        return builder.ConfigureServices(services =>
        {
            services.AddSingleton<MainWindow>();
            services.AddTransient<DeviceView>();
        });
    }
}