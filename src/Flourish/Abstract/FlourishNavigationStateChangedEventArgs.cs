namespace ArkheideSystem.Flourish.Abstract;

/// <summary>Represents current navigation and history state.</summary>
public sealed record FlourishNavigationState(
    string? NavigationKey,
    Type? PageType,
    bool CanGoBack,
    bool CanGoForward
);

/// <summary>Provides data when navigation or navigation history changes.</summary>
public sealed class FlourishNavigationStateChangedEventArgs(
    FlourishNavigationState current
) : EventArgs
{
    /// <summary>Gets the current navigation state.</summary>
    public FlourishNavigationState Current { get; } = current;
}
