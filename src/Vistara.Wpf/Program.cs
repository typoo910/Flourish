using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Vistara.Wpf.Services;
using Vistara.Wpf.Views;

namespace Vistara.Wpf;

internal static class Program
{
    private static IHost? host;

    public static T Fetch<T>()
        where T : class
    {
        return (host?.Services.GetRequiredService(typeof(T)) as T)
            ?? throw new InvalidOperationException("Cannot find service of specified type.");
    }

    [STAThread]
    public static int Main(string[] args)
    {
        ConfigureHost(args);

        try
        {
            host!.Start();
            var app = Fetch<App>();
            return app.Run();
        }
        finally
        {
            host?.StopAsync().GetAwaiter().GetResult();
            host?.Dispose();
        }
    }

    private static void ConfigureHost(string[] args)
    {
        host = Host.CreateDefaultBuilder(args)
            .ConfigureServices(
                (_, services) =>
                {
                    services.AddSingleton<App>();
                    services.AddSingleton<MainWindow>();

                    services.AddSingleton<PageHistoryService>();
                    services.AddSingleton<INavigationService, NavigationService>();

                    services.AddTransient<HomePage>();
                    services.AddTransient<GalleryPage>();
                    services.AddTransient<EditorPage>();
                    services.AddTransient<SettingsPage>();
                }
            )
            .Build();
    }
}
