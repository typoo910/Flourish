namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Provides a title bar search-box state after it changes.
/// </summary>
/// <param name="state">The state after the change.</param>
public sealed class FlourishTitleBarSearchStateChangedEventArgs(
    FlourishTitleBarSearchState state
) : EventArgs
{
    /// <summary>
    /// Gets the state after the change.
    /// </summary>
    public FlourishTitleBarSearchState State { get; } = state;
}
