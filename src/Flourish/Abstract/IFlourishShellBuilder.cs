using System.Windows;

namespace AcksheedSys.Flourish.Abstract;

public interface IFlourishShellBuilder
{
    IFlourishShellBuilder SetTitle(string title);

    IFlourishShellBuilder SetSubtitle(string subtitle);

    IFlourishShellBuilder SetLogo(string packUri);

    IFlourishShellBuilder UseFlourishTitlebar(
        bool EnableSearch = true,
        bool EnableHistoryArrow = true,
        bool EnableNavToggle = true,
        bool EnableLogo = true,
        bool EnableTitle = true,
        bool EnableSubTitle = true,
        bool EnableProfile = true
    );

    IFlourishShellBuilder SetSearchPlaceholder(string placeholder);

    IFlourishShellBuilder SetWindowSize(double width, double height);

    IFlourishShellBuilder SetWindowMinSize(double minWidth, double minHeight);

    IFlourishShellBuilder SetWindowMaxSize(double maxWidth, double maxHeight);

    IFlourishShellBuilder SetWindowPosition(
        WindowStartupLocation startupLocation
    );

    IFlourishShellBuilder SetWindowPosition(double left, double top);

    IFlourishShellBuilder SetWindowState(WindowState windowState);

    IFlourishShellBuilder SetWindowResizeMode(ResizeMode resizeMode);

    IFlourishShellBuilder UseTopmost(bool enabled = true);

    IFlourishShellBuilder ShowInTaskbar(bool enabled = true);

    IFlourishShellBuilder UseNavigationPanel(
        bool enabled = true,
        NavigationPanelDirection direction = NavigationPanelDirection.Left,
        string title = "Navigation"
    );

    IFlourishShellBuilder UseDynamicToolbar(bool enabled = true);

    IFlourishShellBuilder UseBreadcrumb(
        bool enabled = true,
        BreadcrumbShowOption mode = BreadcrumbShowOption.OnlyAvailable
    );
}
