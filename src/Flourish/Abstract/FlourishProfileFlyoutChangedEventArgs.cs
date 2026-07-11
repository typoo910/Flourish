namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Provides the current profile flyout state after it changes.
/// </summary>
/// <param name="state">The state after the change.</param>
public sealed class FlourishProfileFlyoutChangedEventArgs(
    FlourishProfileFlyoutState state
) : EventArgs
{
    /// <summary>
    /// Gets the state after the change.
    /// </summary>
    public FlourishProfileFlyoutState State { get; } = state;
}
