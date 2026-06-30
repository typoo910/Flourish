using System.Windows;

namespace AcksheedSys.Flourish.Abstract;

public interface ITrayIconService
{
    bool IsEnabled { get; }

    bool IsExitRequested { get; }

    void Initialize(Window owner, string tooltipText);

    bool MinimizeToTray();

    void RestoreFromTray();

    void ExitFromTray();
}
