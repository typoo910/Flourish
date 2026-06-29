namespace AcksheedSys.Flourish.Abstract;

public interface IFlourishShellBuilder
{
    IFlourishShellBuilder SetTitle(string title);

    IFlourishShellBuilder SetSubtitle(string subtitle);

    IFlourishShellBuilder SetLogo(string packUri);

    IFlourishShellBuilder UseNavigationPanel(
        bool enabled = true,
        NavigationPanelDirection direction = NavigationPanelDirection.Left,
        string title = "Navigation"
    );

    IFlourishShellBuilder UseSearchOnTitlebar(bool enabled = true, string placeholder = "Search");

    IFlourishShellBuilder UseDynamicToolbar(bool enabled = true);

    IFlourishShellBuilder UseBreadcrumb(
        bool enabled = true,
        BreadcrumbShowOption mode = BreadcrumbShowOption.OnlyAvailable
    );
}
