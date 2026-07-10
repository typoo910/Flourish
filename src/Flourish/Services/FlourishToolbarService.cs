using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Configuration;

namespace ArkheideSystem.Flourish.Services;

internal sealed class FlourishToolbarService(FlourishShellOptions options)
{
    public IReadOnlyList<FlourishToolbarItem> GetToolbarItems(Type? pageType = null)
    {
        if (
            options.IsDynamicToolbarEnabled
            && pageType is not null
            && options.DynamicToolbarItems.TryGetValue(pageType, out var dynamicItems)
        )
        {
            return dynamicItems;
        }

        return options.ToolbarItems;
    }
}
