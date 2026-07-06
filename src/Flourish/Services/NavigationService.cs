using System.Windows.Controls;
using AckSS.Flourish.Abstract;

namespace AckSS.Flourish.Services;

internal sealed class NavigationService(
    PageCacheService pageCacheService,
    PageHistoryService pageHistoryService
) : INavigationService, IFrameNavigationService
{
    private Frame? contentFrame;
    private object? currentParameter;

    public event EventHandler<FlourishNavigatedEventArgs>? Navigated;

    public bool CanGoBack => pageHistoryService.CanGoBack;

    public bool CanGoForward => pageHistoryService.CanGoForward;

    public Type? CurrentSourcePageType { get; private set; }

    public void Initialize(Frame contentFrame)
    {
        this.contentFrame = contentFrame;
    }

    public bool Navigate(Type sourcePageType, object? parameter = null, bool addToBackStack = true)
    {
        return NavigateCore(sourcePageType, parameter, addToBackStack);
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

        if (NavigateCore(entry.SourcePageType, entry.Parameter, false))
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

        if (NavigateCore(entry.SourcePageType, entry.Parameter, false))
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

    private bool NavigateCore(Type sourcePageType, object? parameter, bool addToBackStack)
    {
        if (contentFrame is null)
        {
            throw new InvalidOperationException(
                "NavigationService must be initialized with a frame."
            );
        }

        if (CurrentSourcePageType == sourcePageType && Equals(currentParameter, parameter))
        {
            return false;
        }

        if (addToBackStack && CurrentSourcePageType is not null)
        {
            pageHistoryService.Push(
                new FlourishPageStackEntry(CurrentSourcePageType, currentParameter)
            );
            pageHistoryService.ClearForward();
        }

        var page = pageCacheService.GetPage(sourcePageType);
        contentFrame.Navigate(page);
        CurrentSourcePageType = sourcePageType;
        currentParameter = parameter;

        Navigated?.Invoke(this, new FlourishNavigatedEventArgs(sourcePageType, page, parameter));
        return true;
    }

    private FlourishPageStackEntry? CreateCurrentEntry()
    {
        return CurrentSourcePageType is null
            ? null
            : new FlourishPageStackEntry(CurrentSourcePageType, currentParameter);
    }
}
