using System.Windows.Controls;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Configuration;

namespace ArkheideSystem.Flourish.Services;

internal sealed class PageCacheService : INavigationPageProvider
{
    private readonly IPageFactory pageFactory;
    private readonly Dictionary<Type, Page> cachedPages = [];
    private readonly Dictionary<Type, FlourishPageCacheMode> cacheModesByPageType;

    public PageCacheService(IServiceProvider serviceProvider, FlourishShellOptions options)
        : this(new ServiceProviderPageFactory(serviceProvider), options) { }

    internal PageCacheService(IPageFactory pageFactory, FlourishShellOptions options)
    {
        this.pageFactory = pageFactory ?? throw new ArgumentNullException(nameof(pageFactory));
        ArgumentNullException.ThrowIfNull(options);
        cacheModesByPageType = CreateCacheModeMap(options);
    }

    private static Dictionary<Type, FlourishPageCacheMode> CreateCacheModeMap(
        FlourishShellOptions options
    )
    {
        return new Dictionary<Type, FlourishPageCacheMode>(options.PageCacheModesByPageType);
    }

    public Page GetPage(Type sourcePageType)
    {
        ArgumentNullException.ThrowIfNull(sourcePageType);

        if (
            cacheModesByPageType.TryGetValue(sourcePageType, out var cacheMode)
            && cacheMode == FlourishPageCacheMode.Enabled
        )
        {
            if (!cachedPages.TryGetValue(sourcePageType, out var cachedPage))
            {
                cachedPage = CreatePage(sourcePageType);
                cachedPages[sourcePageType] = cachedPage;
            }

            return cachedPage;
        }

        return CreatePage(sourcePageType);
    }

    private Page CreatePage(Type sourcePageType)
    {
        var page = pageFactory.Create(sourcePageType);

        return page as Page
            ?? throw new InvalidOperationException(
                $"{sourcePageType.FullName} must derive from System.Windows.Controls.Page."
            );
    }
}
