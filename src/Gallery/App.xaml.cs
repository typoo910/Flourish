using System.Windows;

namespace Gallery;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        Program.Flourish.ShowShell(this);
    }
}
