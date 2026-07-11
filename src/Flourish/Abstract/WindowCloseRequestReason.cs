namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Identifies the source of a shell close request.
/// </summary>
public enum WindowCloseRequestReason
{
    /// <summary>
    /// The request originated from a Flourish title bar command.
    /// </summary>
    TitleBar,

    /// <summary>
    /// The request originated from the native window close operation.
    /// </summary>
    Window,

    /// <summary>
    /// The request originated from the notification-area menu.
    /// </summary>
    Tray,

    /// <summary>
    /// The request originated from application code.
    /// </summary>
    Application,
}
