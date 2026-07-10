using System.Windows.Controls;

namespace ArkheideSystem.Flourish.Services;

internal interface INavigationPageProvider
{
    Page GetPage(Type sourcePageType);
}
