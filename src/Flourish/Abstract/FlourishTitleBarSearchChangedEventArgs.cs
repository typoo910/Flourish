namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Describes a title bar search query raised at runtime.
/// </summary>
/// <param name="text">The complete user-entered query.</param>
/// <param name="sequence">The monotonically increasing query sequence number.</param>
public sealed class FlourishTitleBarSearchChangedEventArgs(string text, long sequence) : EventArgs
{
    /// <summary>
    /// Gets the complete user-entered query.
    /// </summary>
    public string Text { get; } = text;

    /// <summary>
    /// Gets the query sequence number, which can be used to reject stale asynchronous results.
    /// </summary>
    public long Sequence { get; } = sequence;
}
