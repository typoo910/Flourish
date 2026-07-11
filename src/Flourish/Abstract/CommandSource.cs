namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Identifies the UI surface or application component that requested a command.
/// </summary>
public enum CommandSource
{
    /// <summary>
    /// The command source is not known.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// The command was requested directly by application code.
    /// </summary>
    Application,

    /// <summary>
    /// The command was requested by a navigation item.
    /// </summary>
    Navigation,

    /// <summary>
    /// The command was requested by a toolbar item.
    /// </summary>
    Toolbar,

    /// <summary>
    /// The command was requested by a title bar item.
    /// </summary>
    TitleBar,

    /// <summary>
    /// The command was requested by a status bar item.
    /// </summary>
    StatusBar,

    /// <summary>
    /// The command was requested by a shell notification.
    /// </summary>
    Notification,

    /// <summary>
    /// The command was requested by a keyboard shortcut.
    /// </summary>
    Shortcut,

    /// <summary>
    /// The command was requested by custom region content.
    /// </summary>
    CustomRegion,
}
