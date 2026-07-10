using System.Windows.Controls;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Configuration;

namespace ArkheideSystem.Flourish.Composition;

internal sealed class FlourishDynamicToolbarBuilder(FlourishShellOptions options)
    : IFlourishDynamicToolbarBuilder
{
    public IFlourishDynamicToolbarBuilder CreateToolbarItems(
        Type pageType,
        params FlourishToolbarItem[] items
    )
    {
        return CreateToolbarItems(pageType, true, items);
    }

    public IFlourishDynamicToolbarBuilder CreateToolbarItems(
        Type pageType,
        bool icon,
        params FlourishToolbarItem[] items
    )
    {
        if (!typeof(Page).IsAssignableFrom(pageType))
        {
            throw new ArgumentException(
                $"{pageType.FullName} must derive from System.Windows.Controls.Page.",
                nameof(pageType)
            );
        }

        options.DynamicToolbarItems[pageType] = items;
        options.DynamicToolbarIconModes[pageType] = icon;
        return this;
    }

    public IFlourishDynamicToolbarBuilder CreateToolbarItems<TPage>(
        params FlourishToolbarItem[] items
    )
        where TPage : Page
    {
        return CreateToolbarItems(typeof(TPage), items);
    }

    public IFlourishDynamicToolbarBuilder CreateToolbarItems<TPage>(
        bool icon,
        params FlourishToolbarItem[] items
    )
        where TPage : Page
    {
        return CreateToolbarItems(typeof(TPage), icon, items);
    }
}
