namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Provides a notification-area state after it changes.
/// </summary>
/// <param name="state">The state after the change.</param>
public sealed class FlourishTrayStateChangedEventArgs(FlourishTrayState state) : EventArgs
{
    /// <summary>
    /// Gets the state after the change.
    /// </summary>
    public FlourishTrayState State { get; } = state;
}
