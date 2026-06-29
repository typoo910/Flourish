using System.Windows.Controls;
using Flourish.Models;

namespace AcksheedSys.Flourish.Abstract;

internal sealed class FlourishNavigationBuilder : IFlourishNavigationBuilder
{
    private readonly FlourishShellOptions options;

    public FlourishNavigationBuilder(FlourishShellOptions options)
    {
        this.options = options;
    }

    public IFlourishNavigationBuilder SetInitialNavigationItem(Type pageType)
    {
        EnsurePageType(pageType);
        options.InitialNavigationPageType = pageType;
        return this;
    }

    public IFlourishNavigationBuilder SetInitialNavigationItem<TPage>()
        where TPage : Page
    {
        return SetInitialNavigationItem(typeof(TPage));
    }

    public IFlourishNavigationBuilder AddNavigationItem(string displayName, string iconGlyph, Type pageType)
    {
        EnsurePageType(pageType);
        var key = pageType.FullName ?? displayName;
        options.NavigationItems.Add(new FlourishNavigationItem(key, displayName, iconGlyph, pageType));
        return this;
    }

    public IFlourishNavigationBuilder AddNavigationItem<TPage>(string displayName, string iconGlyph)
        where TPage : Page
    {
        return AddNavigationItem(displayName, iconGlyph, typeof(TPage));
    }

    private static void EnsurePageType(Type pageType)
    {
        if (!typeof(Page).IsAssignableFrom(pageType))
        {
            throw new ArgumentException($"{pageType.FullName} must derive from System.Windows.Controls.Page.", nameof(pageType));
        }
    }
}
