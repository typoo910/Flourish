using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Vistara.Views;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Title = "Vistara";
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);

        RootFrame.Navigated += RootFrame_Navigated;
        RootNavigationView.SelectedItem = HomeItem;
        NavigateToTag("Home", false);
    }

    private void TitleBar_BackRequested(TitleBar sender, object args)
    {
        if (RootFrame.CanGoBack)
        {
            RootFrame.GoBack();
        }
    }

    private void TitleBar_PaneToggleRequested(TitleBar sender, object args)
    {
        RootNavigationView.IsPaneOpen = !RootNavigationView.IsPaneOpen;
    }

    private void RootNavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItemContainer?.Tag is string tag)
        {
            NavigateToTag(tag, true);
        }
    }

    private void RootFrame_Navigated(object sender, NavigationEventArgs e)
    {
        AppTitleBar.IsBackButtonEnabled = RootFrame.CanGoBack;
        RootNavigationView.SelectedItem = e.SourcePageType.Name switch
        {
            nameof(HomePage) => HomeItem,
            nameof(GalleryPage) => GalleryItem,
            nameof(EditorPage) => EditorItem,
            nameof(SettingsPage) => SettingsItem,
            _ => HomeItem,
        };
    }

    private void NavigateToTag(string tag, bool addToBackStack)
    {
        var targetPage = tag switch
        {
            "Home" => typeof(HomePage),
            "Gallery" => typeof(GalleryPage),
            "Editor" => typeof(EditorPage),
            "Settings" => typeof(SettingsPage),
            _ => typeof(HomePage),
        };

        if (RootFrame.CurrentSourcePageType == targetPage)
        {
            return;
        }

        RootFrame.Navigate(targetPage);

        if (!addToBackStack && RootFrame.CanGoBack)
        {
            RootFrame.BackStack.Clear();
        }
    }
}
