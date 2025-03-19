using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Studio.Contracts.Services;
using Studio.Contracts.Views;
using Studio.Models;
using Studio.Services;
using Studio.Services.ApplicationServices;
using Studio.Services.Data;
using Studio.Services.Files;
using Studio.Services.Storage;
using Studio.Views;

namespace Studio;

// For more information about application lifecycle events see https://docs.microsoft.com/dotnet/framework/wpf/app-development/application-management-overview

// WPF UI elements use language en-US by default.
// If you need to support other cultures make sure you add converters and review dates and numbers in your UI to ensure everything adapts correctly.
// Tracking issue for improving this is https://github.com/dotnet/wpf/issues/1946
public partial class App : Application
{
    private IHost _host;


    public T GetService<T>()
        where T : class
        => _host.Services.GetService(typeof(T)) as T;

    public App()
    {

    }

    private async void OnStartup(object sender, StartupEventArgs e)
    {
        var appLocation = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

        // For more information about .NET generic host see  https://docs.microsoft.com/aspnet/core/fundamentals/host/generic-host?view=aspnetcore-3.0
        _host = Host.CreateDefaultBuilder(e.Args)
                .ConfigureAppConfiguration(c =>
                {
                    c.SetBasePath(appLocation);
                })
                .ConfigureServices(ConfigureServices)
                .Build();

        await _host.StartAsync();
    }

    private void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {

        // App Host
        services.AddHostedService<ApplicationHostService>();

        // Activation Handlers

        // Core Services
        services.AddSingleton<FileService>();

        // Services
        services.AddSingleton<ConfigStorageService>();
        services.AddSingleton<INavigationService, NavigationService>();


        services.AddSingleton<UserProfileDataService, SampleUserProfileDataService>();
        services.AddSingleton<FavouriteProfileDataService, SampleFavouriteProfileDataService>();


        services.AddSingleton<PathResolverService>();
        services.AddSingleton<ProfileDataFetchingService>();
        services.AddSingleton<BattleNetService>();


        // Views
        services.AddTransient<IShellWindow, ShellWindow>();

        services.AddTransient<MainPage>();
        services.AddTransient<PatchNotesPage>();

        services.AddTransient<SettingsPage>();

        services.AddTransient<AccountDetailsPage>();
        services.AddTransient<AccountListPage>();

        // Configuration
        services.Configure<AppConfig>(context.Configuration.GetSection(nameof(AppConfig)));
    }

    private async void OnExit(object sender, ExitEventArgs e)
    {
        await _host.StopAsync();
        _host.Dispose();
        _host = null;
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
    }
}
