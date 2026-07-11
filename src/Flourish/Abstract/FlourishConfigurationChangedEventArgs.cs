namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Describes a change to the effective host configuration.
/// </summary>
public sealed class FlourishConfigurationChangedEventArgs(
    FlourishConfigurationSnapshot previous,
    FlourishConfigurationSnapshot current,
    IReadOnlyList<string> changedKeys
) : EventArgs
{
    /// <summary>
    /// Gets the snapshot that was current before the reload.
    /// </summary>
    public FlourishConfigurationSnapshot Previous { get; } = previous;

    /// <summary>
    /// Gets the newly effective snapshot.
    /// </summary>
    public FlourishConfigurationSnapshot Current { get; } = current;

    /// <summary>
    /// Gets the paths whose effective values changed.
    /// </summary>
    public IReadOnlyList<string> ChangedKeys { get; } = changedKeys;
}
