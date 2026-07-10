using System.Windows.Controls;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Configuration;

namespace ArkheideSystem.Flourish.Services;

internal sealed class NavigationService(
    PageCacheService pageCacheService,
    PageHistoryService pageHistoryService,
    FlourishShellOptions options
) : INavigationService, IFrameNavigationService
{
    private Frame? contentFrame;
    private object? currentParameter;

    public event EventHandler<FlourishNavigatedEventArgs>? Navigated;

    public bool CanGoBack => pageHistoryService.CanGoBack;

    public bool CanGoForward => pageHistoryService.CanGoForward;

    public Type? CurrentSourcePageType { get; private set; }

    public string? CurrentNavigationKey { get; private set; }

    public void Initialize(Frame contentFrame)
    {
        this.contentFrame = contentFrame;
    }

    public bool Navigate(string navigationKey, object? parameter = null, bool addToBackStack = true)
    {
        return NavigateCore(navigationKey, parameter, addToBackStack);
    }

    public bool Navigate(Type sourcePageType, object? parameter = null, bool addToBackStack = true)
    {
        if (!options.NavigationKeysByPageType.TryGetValue(sourcePageType, out var navigationKey))
        {
            throw new InvalidOperationException(
                $"{sourcePageType.FullName} must be registered with AddNavigable before it can be navigated to."
            );
        }

        return Navigate(navigationKey, parameter, addToBackStack);
    }

    public bool Navigate<TPage>(object? parameter = null, bool addToBackStack = true)
        where TPage : Page
    {
        return Navigate(typeof(TPage), parameter, addToBackStack);
    }

    public bool GoBack()
    {
        if (!pageHistoryService.TryPopBack(out var entry))
        {
            return false;
        }

        if (CreateCurrentEntry() is { } currentEntry)
        {
            pageHistoryService.PushForward(currentEntry);
        }

        if (NavigateCore(entry.NavigationKey, entry.Parameter, false))
        {
            return true;
        }

        pageHistoryService.TryPopForward(out _);
        pageHistoryService.Push(entry);
        return false;
    }

    public bool GoForward()
    {
        if (!pageHistoryService.TryPopForward(out var entry))
        {
            return false;
        }

        if (CreateCurrentEntry() is { } currentEntry)
        {
            pageHistoryService.Push(currentEntry);
        }

        if (NavigateCore(entry.NavigationKey, entry.Parameter, false))
        {
            return true;
        }

        pageHistoryService.TryPopBack(out _);
        pageHistoryService.PushForward(entry);
        return false;
    }

    public void ClearBackStack()
    {
        pageHistoryService.Clear();
    }

    private bool NavigateCore(string navigationKey, object? parameter, bool addToBackStack)
    {
        if (contentFrame is null)
        {
            throw new InvalidOperationException(
                "NavigationService must be initialized with a frame."
            );
        }

        if (!options.PageTypesByNavigationKey.TryGetValue(navigationKey, out var sourcePageType))
        {
            throw new InvalidOperationException(
                $"Navigation key '{navigationKey}' must be registered with AddNavigable before it can be navigated to."
            );
        }

        if (CurrentNavigationKey == navigationKey && Equals(currentParameter, parameter))
        {
            return false;
        }

        if (addToBackStack && CurrentNavigationKey is not null)
        {
            pageHistoryService.Push(
                new FlourishPageStackEntry(CurrentNavigationKey, currentParameter)
            );
            pageHistoryService.ClearForward();
        }

        var page = pageCacheService.GetPage(sourcePageType);
        contentFrame.Navigate(page);
        CurrentSourcePageType = sourcePageType;
        CurrentNavigationKey = navigationKey;
        currentParameter = parameter;

        Navigated?.Invoke(
            this,
            new FlourishNavigatedEventArgs(navigationKey, sourcePageType, page, parameter)
        );
        return true;
    }

    private FlourishPageStackEntry? CreateCurrentEntry()
    {
        return CurrentNavigationKey is null
            ? null
            : new FlourishPageStackEntry(CurrentNavigationKey, currentParameter);
    }
}
