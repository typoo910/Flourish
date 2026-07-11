using System.Collections.ObjectModel;

namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Provides an immutable keyboard shortcut snapshot after a structural change.
/// </summary>
public sealed class ShortcutRegistryChangedEventArgs : EventArgs
{
    internal ShortcutRegistryChangedEventArgs(
        long version,
        ShortcutRegistryChangeKind changeKind,
        ShortcutRegistrationInfo affectedShortcut,
        IEnumerable<ShortcutRegistrationInfo> registrations
    )
    {
        Version = version;
        ChangeKind = changeKind;
        AffectedShortcut = affectedShortcut;
        Registrations = new ReadOnlyCollection<ShortcutRegistrationInfo>(
            registrations.ToArray()
        );
    }

    /// <summary>
    /// Gets the monotonically increasing shortcut registry version.
    /// </summary>
    public long Version { get; }

    /// <summary>
    /// Gets the kind of structural change.
    /// </summary>
    public ShortcutRegistryChangeKind ChangeKind { get; }

    /// <summary>
    /// Gets the shortcut added or removed by the change.
    /// </summary>
    public ShortcutRegistrationInfo AffectedShortcut { get; }

    /// <summary>
    /// Gets all active shortcuts in registration order.
    /// </summary>
    public IReadOnlyList<ShortcutRegistrationInfo> Registrations { get; }
}
