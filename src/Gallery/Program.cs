using System.Windows;
using AcksheedSys.Flourish.Abstract;
using Gallery.Views;
using Microsoft.Extensions.DependencyInjection;

namespace Gallery;

internal static class Program
{
    private static IFlourish? flourish;

    public static IFlourish Flourish =>
        flourish ?? throw new InvalidOperationException("Flourish has not been built.");

    public static T Fetch<T>()
        where T : notnull
    {
        return Flourish.GetRequiredService<T>();
    }

    [STAThread]
    public static int Main(string[] args)
    {
        flourish = FlourishBuilder
            .CreateDefaultBuilder(args)
            .ConfigureServices(
                (_, services) =>
                {
                    services.AddSingleton<App>();

                    services.AddNavigablePage<HomePage>("首页", "\uE80F", isInitial: true);
                    services.AddNavigablePage<GalleryPage>("图库", "\uE91B");
                    services.AddNavigablePage<EditorPage>("编辑", "\uE70F");
                    services.AddNavigablePage<SettingsPage>("设置", "\uE713");
                }
            )
            .ConfigureShell(
                (_, shell) =>
                {
                    shell
                        .SetTitle("Gallery")
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
            .ConfigureDynamicToolbar(
                (_, tool) =>
                {
                    tool.CreateToolbarItems<HomePage>(
                        new("打开", "\uE8E5", () => MessageBox.Show("Hello Home Page")),
                        new("保存", "\uE74E", () => MessageBox.Show("Hello Home Page"))
                    );

                    tool.CreateToolbarItems<GalleryPage>(
                        new("打开", "\uE8E5", () => MessageBox.Show("Hello Gallery Page")),
                        new("保存", "\uE74E", () => MessageBox.Show("Hello Gallery Page")),
                        new("导入", "\uE898", () => MessageBox.Show("Hello Gallery Page"))
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
            flourish.Start();
            var app = flourish.GetRequiredService<App>();
            return app.Run();
        }
        finally
        {
            flourish.StopAsync().GetAwaiter().GetResult();
            flourish.Dispose();
            flourish = null;
        }
    }
}
