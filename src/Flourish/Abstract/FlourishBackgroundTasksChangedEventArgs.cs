using System.Collections.ObjectModel;

namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Provides the current immutable background task list after it changes.
/// </summary>
public sealed class FlourishBackgroundTasksChangedEventArgs : EventArgs
{
    internal FlourishBackgroundTasksChangedEventArgs(
        IEnumerable<FlourishBackgroundTaskInfo> tasks
    )
    {
        Tasks = new ReadOnlyCollection<FlourishBackgroundTaskInfo>(tasks.ToArray());
    }

    /// <summary>
    /// Gets the queued, running, and cancelling tasks in submission order.
    /// </summary>
    public IReadOnlyList<FlourishBackgroundTaskInfo> Tasks { get; }
}
