using System.Windows.Controls;
using Flourish.Internal;
using Flourish.Models;

namespace AcksheedSys.Flourish.Abstract;

internal sealed class FlourishDynamicToolbarBuilder : IFlourishDynamicToolbarBuilder
{
    private readonly FlourishShellOptions options;

    public FlourishDynamicToolbarBuilder(FlourishShellOptions options)
    {
        this.options = options;
    }

    public IFlourishDynamicToolbarBuilder CreateToolbarItems(Type pageType, params FlourishToolbarItem[] items)
    {
        if (!typeof(Page).IsAssignableFrom(pageType))
        {
            throw new ArgumentException($"{pageType.FullName} must derive from System.Windows.Controls.Page.", nameof(pageType));
        }

        options.DynamicToolbarItems[pageType] = items.Select(ToCommandItem).ToArray();
        return this;
    }

    public IFlourishDynamicToolbarBuilder CreateToolbarItems<TPage>(params FlourishToolbarItem[] items)
        where TPage : Page
    {
        return CreateToolbarItems(typeof(TPage), items);
    }

    private static FlourishCommandItem ToCommandItem(FlourishToolbarItem item)
    {
        return new FlourishCommandItem(
            item.DisplayName,
            item.IconGlyph,
            item.Action is null ? null : new ActionCommand(item.Action)
        );
    }
}
