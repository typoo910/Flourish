using System.Windows.Controls;
using AcksheedSys.Flourish.Abstract;

namespace AcksheedSys.Flourish.Models;

internal sealed record FlourishNavigationItem(
    string Key,
    string Label,
    string IconGlyph,
    Type PageType,
    FlourishPageCacheMode CacheMode
)
{
    public void Validate()
    {
        if (!typeof(Page).IsAssignableFrom(PageType))
        {
            throw new ArgumentException(
                $"{PageType.FullName} must derive from System.Windows.Controls.Page.",
                nameof(PageType)
            );
        }
    }
}
