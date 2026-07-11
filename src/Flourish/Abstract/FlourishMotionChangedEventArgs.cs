namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Describes a runtime animation settings change.
/// </summary>
public sealed class FlourishMotionChangedEventArgs(
    FlourishMotionSettings previous,
    FlourishMotionSettings current,
    bool canAnimate
) : EventArgs
{
    /// <summary>
    /// Gets the settings before the change.
    /// </summary>
    public FlourishMotionSettings Previous { get; } = previous;

    /// <summary>
    /// Gets the settings after the change.
    /// </summary>
    public FlourishMotionSettings Current { get; } = current;

    /// <summary>
    /// Gets whether animation is currently allowed.
    /// </summary>
    public bool CanAnimate { get; } = canAnimate;
}
