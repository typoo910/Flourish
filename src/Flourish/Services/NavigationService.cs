using System.Windows.Controls;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Configuration;

namespace ArkheideSystem.Flourish.Services;

internal sealed class NavigationService : INavigationService, IFrameNavigationService
{
    private readonly INavigationPageProvider pageProvider;
    private readonly PageHistoryService pageHistoryService;
    private readonly FlourishShellOptions options;
    private INavigationContentHost? contentHost;
    private object? currentParameter;

    public NavigationService(
        PageCacheService pageCacheService,
        PageHistoryService pageHistoryService,
        FlourishShellOptions options
    )
        : this((INavigationPageProvider)pageCacheService, pageHistoryService, options) { }

    internal NavigationService(
        INavigationPageProvider pageProvider,
        PageHistoryService pageHistoryService,
        FlourishShellOptions options
    )
    {
        this.pageProvider = pageProvider ?? throw new ArgumentNullException(nameof(pageProvider));
        this.pageHistoryService =
            pageHistoryService ?? throw new ArgumentNullException(nameof(pageHistoryService));
        this.options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public event EventHandler<FlourishNavigatedEventArgs>? Navigated;

    public bool CanGoBack => pageHistoryService.CanGoBack;

    public bool CanGoForward => pageHistoryService.CanGoForward;

    public Type? CurrentSourcePageType { get; private set; }

    public string? CurrentNavigationKey { get; private set; }

    public void Initialize(Frame contentFrame)
    {
        Initialize(new FrameNavigationContentHost(contentFrame));
    }

    internal void Initialize(INavigationContentHost contentHost)
    {
        this.contentHost = contentHost ?? throw new ArgumentNullException(nameof(contentHost));
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

        var currentEntry = CreateCurrentEntry();
        return NavigateCore(
            entry.NavigationKey,
            entry.Parameter,
            false,
            commitHistory: () =>
            {
                if (currentEntry is not null)
                {
                    pageHistoryService.PushForward(currentEntry);
                }
            },
            rollbackHistory: () => pageHistoryService.Push(entry)
        );
    }

    public bool GoForward()
    {
        if (!pageHistoryService.TryPopForward(out var entry))
        {
            return false;
        }

        var currentEntry = CreateCurrentEntry();
        return NavigateCore(
            entry.NavigationKey,
            entry.Parameter,
            false,
            commitHistory: () =>
            {
                if (currentEntry is not null)
                {
                    pageHistoryService.Push(currentEntry);
                }
            },
            rollbackHistory: () => pageHistoryService.PushForward(entry)
        );
    }

    public void ClearBackStack()
    {
        pageHistoryService.ClearBack();
    }

    private bool NavigateCore(
        string navigationKey,
        object? parameter,
        bool addToBackStack,
        Action? commitHistory = null,
        Action? rollbackHistory = null
    )
    {
        var historyCommitted = false;

        try
        {
            if (contentHost is null)
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
                rollbackHistory?.Invoke();
                return false;
            }

            var page = pageProvider.GetPage(sourcePageType);
            if (!contentHost.Navigate(page))
            {
                rollbackHistory?.Invoke();
                return false;
            }

            commitHistory?.Invoke();
            if (addToBackStack && CreateCurrentEntry() is { } currentEntry)
            {
                pageHistoryService.Push(currentEntry);
                pageHistoryService.ClearForward();
            }

            historyCommitted = true;
            CurrentSourcePageType = sourcePageType;
            CurrentNavigationKey = navigationKey;
            currentParameter = parameter;

            Navigated?.Invoke(
                this,
                new FlourishNavigatedEventArgs(navigationKey, sourcePageType, page, parameter)
            );
            return true;
        }
        catch
        {
            if (!historyCommitted)
            {
                rollbackHistory?.Invoke();
            }

            throw;
        }
    }

    private FlourishPageStackEntry? CreateCurrentEntry()
    {
        return CurrentNavigationKey is null
            ? null
            : new FlourishPageStackEntry(CurrentNavigationKey, currentParameter);
    }
}
