namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Provides the current shell window snapshot after it changes.
/// </summary>
/// <param name="state">The state after the change.</param>
public sealed class FlourishWindowStateChangedEventArgs(FlourishWindowState state) : EventArgs
{
    /// <summary>
    /// Gets the state after the change.
    /// </summary>
    public FlourishWindowState State { get; } = state;
}
