using System.Windows.Controls;

namespace AcksheedSys.Flourish.Abstract;

public interface IFlourishNavigationBuilder
{
    IFlourishNavigationBuilder SetInitialNavigationItem(Type pageType);

    IFlourishNavigationBuilder SetInitialNavigationItem<TPage>()
        where TPage : Page;

    IFlourishNavigationBuilder AddNavigationItem(string displayName, string iconGlyph, Type pageType);

    IFlourishNavigationBuilder AddNavigationItem<TPage>(string displayName, string iconGlyph)
        where TPage : Page;
}
