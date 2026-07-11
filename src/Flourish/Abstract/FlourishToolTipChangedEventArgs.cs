namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Describes a runtime tooltip settings change.
/// </summary>
public sealed class FlourishToolTipChangedEventArgs(
    FlourishToolTipSettings previous,
    FlourishToolTipSettings current
) : EventArgs
{
    /// <summary>
    /// Gets the settings before the change.
    /// </summary>
    public FlourishToolTipSettings Previous { get; } = previous;

    /// <summary>
    /// Gets the settings after the change.
    /// </summary>
    public FlourishToolTipSettings Current { get; } = current;
}
