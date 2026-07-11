using System.Windows.Controls;
using ArkheideSystem.Flourish.Abstract;

namespace ArkheideSystem.Gallery.Views;

public partial class HomePage : Page
{
    private readonly INavigationService navigation;

    public HomePage(INavigationService navigation)
    {
        this.navigation = navigation;
        InitializeComponent();
    }

    private void DemoCard_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        if (sender is Button { Tag: string route })
        {
            navigation.Navigate(route);
        }
    }
}
