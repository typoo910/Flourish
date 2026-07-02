using AcksheedSys.Flourish.Abstract;
using AcksheedSys.Flourish.Configuration;

namespace AcksheedSys.Flourish.Services;

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
