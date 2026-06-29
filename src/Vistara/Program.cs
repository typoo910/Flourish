using System.Windows;
using AcksheedSys.Flourish.Abstract;
using Microsoft.Extensions.DependencyInjection;
using Vistara.Views;

namespace Vistara;

internal static class Program
{
    private static IFlourish? _flourish;
    public static IFlourish Flourish =>
        _flourish ?? throw new InvalidOperationException("Flourish has not been built.");

    public static T Fetch<T>()
        where T : notnull
    {
        return Flourish.GetRequiredService<T>();
    }

    [STAThread]
    public static int Main(string[] args)
    {
        _flourish = FlourishBuilder
            .CreateDefaultBuilder(args)
            .ConfigureServices(
                (_, services) =>
                {
                    services.AddSingleton<App>();

                    services.AddTransient<HomePage>();
                    services.AddTransient<GalleryPage>();
                    services.AddTransient<EditorPage>();
                    services.AddTransient<SettingsPage>();
                }
            )
            .ConfigureShell(
                (_, shell) =>
                {
                    shell
                        .SetTitle("Vistara")
                        .SetSubtitle("图片管理器")
                        .UseNavigationPanel(
                            enabled: true,
                            direction: NavigationPanelDirection.Left,
                            title: "导航"
                        )
                        .UseSearchOnTitlebar(enabled: true, placeholder: "搜索图片")
                        .UseDynamicToolbar()
                        .UseBreadcrumb(enabled: true, mode: BreadcrumbShowOption.OnlyAvailable);
                }
            )
            .ConfigureNavigation(
                (_, nav) =>
                {
                    nav.SetInitialNavigationItem<HomePage>()
                        .AddNavigationItem<HomePage>("首页", "\uE80F")
                        .AddNavigationItem<GalleryPage>("图库", "\uE91B")
                        .AddNavigationItem<EditorPage>("编辑", "\uE70F")
                        .AddNavigationItem<SettingsPage>("设置", "\uE713");
                }
            )
            .ConfigureDynamicToolbar(
                (_, tool) =>
                {
                    tool.CreateToolbarItems<HomePage>(
                        new FlourishToolbarItem(
                            "打开",
                            "\uE8E5",
                            () => MessageBox.Show("Hello Home Page")
                        ),
                        new FlourishToolbarItem(
                            "保存",
                            "\uE74E",
                            () => MessageBox.Show("Hello Home Page")
                        )
                    );

                    tool.CreateToolbarItems<GalleryPage>(
                        new FlourishToolbarItem(
                            "打开",
                            "\uE8E5",
                            () => MessageBox.Show("Hello Gallery Page")
                        ),
                        new FlourishToolbarItem(
                            "保存",
                            "\uE74E",
                            () => MessageBox.Show("Hello Gallery Page")
                        ),
                        new FlourishToolbarItem(
                            "导入",
                            "\uE898",
                            () => MessageBox.Show("Hello Gallery Page")
                        )
                    );
                }
            )
            .ConfigureStatus(
                (_, status) =>
                {
                    status.SetStatusText("就绪").ShowLANConnectionStatus().ShowPowerStatus();
                }
            )
            .Build();

        try
        {
            _flourish.Start();
            var app = _flourish.GetRequiredService<App>();
            return app.Run();
        }
        finally
        {
            _flourish.StopAsync().GetAwaiter().GetResult();
            _flourish.Dispose();
            _flourish = null;
        }
    }
}
