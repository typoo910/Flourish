namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Identifies a change made to the runtime command registry.
/// </summary>
public enum CommandRegistryChangeKind
{
    /// <summary>
    /// A registration was added.
    /// </summary>
    Registered = 0,

    /// <summary>
    /// Existing registrations were replaced.
    /// </summary>
    Replaced,

    /// <summary>
    /// A registration was removed.
    /// </summary>
    Unregistered,
}
