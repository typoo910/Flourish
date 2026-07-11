namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Specifies the default behavior of a Flourish shell close request.
/// </summary>
public enum WindowCloseBehavior
{
    /// <summary>
    /// Uses the shell's normal confirmation flow before exiting.
    /// </summary>
    Prompt,

    /// <summary>
    /// Closes the application without the standard confirmation prompt after guards allow it.
    /// </summary>
    Close,

    /// <summary>
    /// Hides the shell window in the notification area instead of exiting.
    /// </summary>
    MinimizeToTray,
}
