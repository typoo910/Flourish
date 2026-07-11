namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Provides the latest active notification collection.
/// </summary>
/// <param name="notifications">An immutable, creation-ordered snapshot of active notifications.</param>
public sealed class FlourishNotificationsChangedEventArgs(
    IReadOnlyList<FlourishNotificationInfo> notifications
) : EventArgs
{
    /// <summary>
    /// Gets an immutable, creation-ordered snapshot of active notifications.
    /// </summary>
    public IReadOnlyList<FlourishNotificationInfo> Notifications { get; } = notifications;
}
