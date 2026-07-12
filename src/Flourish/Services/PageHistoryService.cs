namespace ArkheideSystem.Flourish.Services;

internal sealed class PageHistoryService
{
    internal const int DefaultMaximumEntries = 100;

    private readonly LinkedList<FlourishPageStackEntry> backStack = new();
    private readonly LinkedList<FlourishPageStackEntry> forwardStack = new();
    private readonly int maximumEntries;

    public PageHistoryService()
        : this(DefaultMaximumEntries) { }

    internal PageHistoryService(int maximumEntries)
    {
        if (maximumEntries <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximumEntries),
                maximumEntries,
                "The navigation history capacity must be greater than zero."
            );
        }

        this.maximumEntries = maximumEntries;
    }

    public bool CanGoBack => backStack.Count > 0;

    public bool CanGoForward => forwardStack.Count > 0;

    public IReadOnlyCollection<FlourishPageStackEntry> BackStack => backStack;

    public IReadOnlyCollection<FlourishPageStackEntry> ForwardStack => forwardStack;

    public void Push(FlourishPageStackEntry entry)
    {
        Push(backStack, entry);
    }

    public void PushForward(FlourishPageStackEntry entry)
    {
        Push(forwardStack, entry);
    }

    public bool TryPopBack(out FlourishPageStackEntry entry)
    {
        if (backStack.Count == 0)
        {
            entry = default!;
            return false;
        }

        entry = backStack.First!.Value;
        backStack.RemoveFirst();
        return true;
    }

    public bool TryPopForward(out FlourishPageStackEntry entry)
    {
        if (forwardStack.Count == 0)
        {
            entry = default!;
            return false;
        }

        entry = forwardStack.First!.Value;
        forwardStack.RemoveFirst();
        return true;
    }

    public void ClearForward()
    {
        forwardStack.Clear();
    }

    public void ClearBack()
    {
        backStack.Clear();
    }

    public void Clear()
    {
        backStack.Clear();
        forwardStack.Clear();
    }

    public void Remove(string navigationKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(navigationKey);
        RemoveFromHistory(backStack, navigationKey);
        RemoveFromHistory(forwardStack, navigationKey);
    }

    private void Push(
        LinkedList<FlourishPageStackEntry> history,
        FlourishPageStackEntry entry
    )
    {
        ArgumentNullException.ThrowIfNull(entry);
        history.AddFirst(entry);
        if (history.Count > maximumEntries)
        {
            history.RemoveLast();
        }
    }

    private static void RemoveFromHistory(
        LinkedList<FlourishPageStackEntry> history,
        string navigationKey
    )
    {
        var node = history.First;
        while (node is not null)
        {
            var next = node.Next;
            if (StringComparer.Ordinal.Equals(node.Value.NavigationKey, navigationKey))
            {
                history.Remove(node);
            }

            node = next;
        }
    }
}
