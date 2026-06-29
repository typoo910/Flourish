using System.Windows;
using Flourish.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AcksheedSys.Flourish.Abstract;

internal sealed class FlourishRuntime : IFlourish
{
    private readonly IHost host;

    public FlourishRuntime(IHost host)
    {
        this.host = host;
    }

    public IServiceProvider Services => host.Services;

    public T GetRequiredService<T>()
        where T : notnull
    {
        return host.Services.GetRequiredService<T>();
    }

    public void Start()
    {
        host.Start();
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        return host.StopAsync(cancellationToken);
    }

    public void ShowShell(Application application)
    {
        EnsureApplicationResources(application);

        var mainWindow = host.Services.GetRequiredService<FlourishShellWindow>();
        application.MainWindow = mainWindow;
        mainWindow.Show();
    }

    public void Dispose()
    {
        host.Dispose();
    }

    private static void EnsureApplicationResources(Application application)
    {
        AddResourceDictionary(application, "/Flourish;component/Themes/Colors.xaml");
        AddResourceDictionary(application, "/Flourish;component/Themes/Controls.xaml");
    }

    private static void AddResourceDictionary(Application application, string source)
    {
        if (application.Resources.MergedDictionaries.Any(dictionary => dictionary.Source?.OriginalString == source))
        {
            return;
        }

        application.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri(source, UriKind.Relative) });
    }
}
