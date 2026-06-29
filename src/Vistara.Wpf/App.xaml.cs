using System.Windows;
using Vistara.Wpf.Views;

namespace Vistara.Wpf;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var mainWindow = Program.Fetch<MainWindow>();
        MainWindow = mainWindow;
        mainWindow.Show();
    }
}
