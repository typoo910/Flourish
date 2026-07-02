using System.Windows;

namespace AcksheedSys.Flourish.Abstract;

public interface IFlourishWindowPropertyBuilder
{
    IFlourishWindowPropertyBuilder SetWindowSize(double width = 1536, double height = 864);

    IFlourishWindowPropertyBuilder SetWindowMinSize(double minWidth = 1280, double minHeight = 720);

    IFlourishWindowPropertyBuilder SetWindowMaxSize(
        double maxWidth = double.PositiveInfinity,
        double maxHeight = double.PositiveInfinity
    );

    IFlourishWindowPropertyBuilder SetWindowPosition(
        WindowStartupLocation startupLocation = WindowStartupLocation.CenterScreen
    );

    IFlourishWindowPropertyBuilder SetManualWindowPosition(double left = 0, double top = 0);

    IFlourishWindowPropertyBuilder SetWindowState(WindowState windowState = WindowState.Normal);

    IFlourishWindowPropertyBuilder SetWindowResizeMode(ResizeMode resizeMode = ResizeMode.CanResize);

    IFlourishWindowPropertyBuilder UseTopmost(bool enabled = true);

    IFlourishWindowPropertyBuilder ShowInTaskbar(bool enabled = true);
}
