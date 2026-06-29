using System.Windows.Controls;
using AcksheedSys.Flourish.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace AcksheedSys.Flourish.Abstract;

public static class FlourishServiceCollectionExtensions
{
    public static IServiceCollection AddNavigable<TPage>(
        this IServiceCollection services,
        string displayName,
        string iconGlyph,
        bool isInitial = false,
        FlourishPageCacheMode cacheMode = FlourishPageCacheMode.Enabled
    )
        where TPage : Page
    {
        services.AddTransient<TPage>();
        GetOrCreateState(services)
            .NavigablePages.Add(
                new NavigablePageRegistration(
                    typeof(TPage),
                    displayName,
                    iconGlyph,
                    isInitial,
                    cacheMode
                )
            );

        return services;
    }

    public static IServiceCollection AddNavigable(
        this IServiceCollection services,
        Type pageType,
        string displayName,
        string iconGlyph,
        bool isInitial = false,
        FlourishPageCacheMode cacheMode = FlourishPageCacheMode.Enabled
    )
    {
        if (!typeof(Page).IsAssignableFrom(pageType))
        {
            throw new ArgumentException(
                $"{pageType.FullName} must derive from System.Windows.Controls.Page.",
                nameof(pageType)
            );
        }

        services.AddTransient(pageType);
        GetOrCreateState(services)
            .NavigablePages.Add(
                new NavigablePageRegistration(pageType, displayName, iconGlyph, isInitial, cacheMode)
            );

        return services;
    }

    private static FlourishServiceCollectionState GetOrCreateState(IServiceCollection services)
    {
        var descriptor = services.FirstOrDefault(descriptor =>
            descriptor.ServiceType == typeof(FlourishServiceCollectionState)
            && descriptor.ImplementationInstance is FlourishServiceCollectionState
        );

        if (descriptor?.ImplementationInstance is FlourishServiceCollectionState state)
        {
            return state;
        }

        state = new FlourishServiceCollectionState();
        services.AddSingleton(state);
        return state;
    }
}
