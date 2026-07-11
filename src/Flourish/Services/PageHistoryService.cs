namespace ArkheideSystem.Flourish.Services;

internal sealed class PageHistoryService
{
    private readonly Stack<FlourishPageStackEntry> backStack = new();
    private readonly Stack<FlourishPageStackEntry> forwardStack = new();

    public bool CanGoBack => backStack.Count > 0;

    public bool CanGoForward => forwardStack.Count > 0;

    public IReadOnlyCollection<FlourishPageStackEntry> BackStack => backStack;

    public IReadOnlyCollection<FlourishPageStackEntry> ForwardStack => forwardStack;

    public void Push(FlourishPageStackEntry entry)
    {
        backStack.Push(entry);
    }

    public void PushForward(FlourishPageStackEntry entry)
    {
        forwardStack.Push(entry);
    }

    public bool TryPopBack(out FlourishPageStackEntry entry)
    {
        if (backStack.Count == 0)
        {
            entry = default!;
            return false;
        }

        entry = backStack.Pop();
        return true;
    }

    public bool TryPopForward(out FlourishPageStackEntry entry)
    {
        if (forwardStack.Count == 0)
        {
            entry = default!;
            return false;
        }

        entry = forwardStack.Pop();
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
        RemoveFromStack(backStack, navigationKey);
        RemoveFromStack(forwardStack, navigationKey);
    }

    private static void RemoveFromStack(
        Stack<FlourishPageStackEntry> stack,
        string navigationKey
    )
    {
        if (stack.Count == 0)
        {
            return;
        }

        var retained = stack
            .Where(entry => !StringComparer.Ordinal.Equals(entry.NavigationKey, navigationKey))
            .Reverse()
            .ToArray();
        stack.Clear();
        foreach (var entry in retained)
        {
            stack.Push(entry);
        }
    }
}
