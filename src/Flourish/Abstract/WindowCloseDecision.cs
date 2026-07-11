namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Represents the result returned by a runtime close guard.
/// </summary>
public enum WindowCloseDecision
{
    /// <summary>
    /// Allows close evaluation to continue to the next guard.
    /// </summary>
    Allow,

    /// <summary>
    /// Stops close evaluation and keeps the application open.
    /// </summary>
    Cancel,
}
