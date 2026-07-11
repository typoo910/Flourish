namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Identifies a change made to the keyboard shortcut registry.
/// </summary>
public enum ShortcutRegistryChangeKind
{
    /// <summary>
    /// A shortcut was registered.
    /// </summary>
    Registered = 0,

    /// <summary>
    /// Conflicting shortcuts were replaced.
    /// </summary>
    Replaced,

    /// <summary>
    /// A shortcut was unregistered.
    /// </summary>
    Unregistered,
}
