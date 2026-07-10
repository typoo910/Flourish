using System.Windows.Controls;

namespace ArkheideSystem.Flourish.Services;

internal interface INavigationContentHost
{
    bool Navigate(Page page);
}
