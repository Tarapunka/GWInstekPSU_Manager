using GWInstekPSUManager.Core.Interfaces.ComPortsInterfaces;
using GWInstekPSUManager.Core.Interfaces.DeviceInterfaces;
using GWInstekPSUManager.HostBuilders;
using GWInstekPSUManager.Infrastructure.Services.ComPortServices;
using GWInstekPSUManager.ViewModels;
using GWInstekPSUManager.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Windows;

namespace GWInstekPSUManager;

public partial class App : Application
{
    private readonly IHost _host;

    public static IServiceProvider ServiceProvider { get; private set; } = null!;

    public App()
    {
        _host = Host.CreateDefaultBuilder()
            .AddAppServices()  // Регистрация сервисов
            .AddAppFactories() // Решистрация Фабрик
            .AddAppLoggers()    // Регистрация логирований
            .AddViewModels()   // Регистрация ViewModel
            .AddViews()        // Регистрация окон
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        await _host.StartAsync();


        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        mainWindow.DataContext = _host.Services.GetRequiredService<MainViewModel>();
        mainWindow.Show();

        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        await _host.StopAsync();
        _host.Dispose();
    }
}