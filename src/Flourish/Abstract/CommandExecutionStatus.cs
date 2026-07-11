namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Describes the outcome of a command dispatch operation.
/// </summary>
public enum CommandExecutionStatus
{
    /// <summary>
    /// No handler accepted the command.
    /// </summary>
    NotHandled = 0,

    /// <summary>
    /// A handler completed the command successfully.
    /// </summary>
    Handled,

    /// <summary>
    /// The command is registered but is currently disabled.
    /// </summary>
    Disabled,

    /// <summary>
    /// A handler or can-execute predicate failed.
    /// </summary>
    Failed,

    /// <summary>
    /// Dispatch was canceled.
    /// </summary>
    Canceled,
}
