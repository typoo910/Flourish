namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Configures keyboard shortcut registration and conflict behavior.
/// </summary>
public sealed class ShortcutRegistrationOptions
{
    /// <summary>
    /// Gets or initializes the shortcut scope.
    /// </summary>
    public ShortcutScope Scope { get; init; } = ShortcutScope.Application;

    /// <summary>
    /// Gets or initializes an optional window or page key. A missing key matches every active member of that scope.
    /// </summary>
    public string? ScopeKey { get; init; }

    /// <summary>
    /// Gets or initializes the policy applied to an identical gesture and scope.
    /// </summary>
    public ShortcutConflictPolicy ConflictPolicy { get; init; } =
        ShortcutConflictPolicy.Reject;

    /// <summary>
    /// Gets or initializes the shortcut priority. Higher-priority shortcuts win within the same scope.
    /// </summary>
    public int Priority { get; init; }
}
