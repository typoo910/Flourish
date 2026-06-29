using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Vistara.Wpf.Services;

namespace Vistara.Wpf.Views;

public partial class MainWindow : Window
{
    private readonly INavigationService navigationService;
    private readonly Dictionary<string, Type> pagesByTag;
    private readonly Dictionary<Type, RadioButton> navigationItemsByPage;
    private readonly TextBlock[] paneLabels;
    private bool isPaneOpen = true;

    public MainWindow(INavigationService navigationService)
    {
        InitializeComponent();

        this.navigationService = navigationService;
        pagesByTag = new Dictionary<string, Type>
        {
            ["Home"] = typeof(HomePage),
            ["Gallery"] = typeof(GalleryPage),
            ["Editor"] = typeof(EditorPage),
            ["Settings"] = typeof(SettingsPage),
        };
        navigationItemsByPage = new Dictionary<Type, RadioButton>
        {
            [typeof(HomePage)] = HomeItem,
            [typeof(GalleryPage)] = GalleryItem,
            [typeof(EditorPage)] = EditorItem,
            [typeof(SettingsPage)] = SettingsItem,
        };
        paneLabels = [HomeLabel, GalleryLabel, EditorLabel, SettingsLabel];

        Title = "Vistara";
        StateChanged += MainWindow_StateChanged;
        navigationService.Initialize(RootFrame);
        navigationService.Navigated += RootFrame_Navigated;

        HomeItem.IsChecked = true;
        NavigateToTag("Home", false);
    }

    private void TitleBar_BackRequested(object sender, RoutedEventArgs e)
    {
        if (navigationService.CanGoBack)
        {
            navigationService.GoBack();
        }
    }

    private void TitleBar_PaneToggleRequested(object sender, RoutedEventArgs e)
    {
        isPaneOpen = !isPaneOpen;
        PaneColumn.Width = new GridLength(isPaneOpen ? 220 : 56);
        PaneTitle.Visibility = isPaneOpen ? Visibility.Visible : Visibility.Collapsed;

        foreach (var label in paneLabels)
        {
            label.Visibility = isPaneOpen ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private void NavigationItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { Tag: string tag })
        {
            NavigateToTag(tag, true);
        }
    }

    private void RootFrame_Navigated(object? sender, PageNavigatedEventArgs e)
    {
        BackButton.IsEnabled = navigationService.CanGoBack;

        if (navigationItemsByPage.TryGetValue(e.SourcePageType, out var navigationItem))
        {
            navigationItem.IsChecked = true;
        }
    }

    private void NavigateToTag(string tag, bool addToBackStack)
    {
        if (!pagesByTag.TryGetValue(tag, out var targetPage))
        {
            targetPage = typeof(HomePage);
        }

        if (navigationService.CurrentSourcePageType == targetPage)
        {
            return;
        }

        navigationService.Navigate(targetPage, addToBackStack: addToBackStack);

        if (!addToBackStack && navigationService.CanGoBack)
        {
            navigationService.ClearBackStack();
            BackButton.IsEnabled = false;
        }
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState != MouseButtonState.Pressed)
        {
            return;
        }

        if (e.ClickCount == 2)
        {
            ToggleWindowState();
            return;
        }

        DragMove();
    }

    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void MaximizeButton_Click(object sender, RoutedEventArgs e)
    {
        ToggleWindowState();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void MainWindow_StateChanged(object? sender, EventArgs e)
    {
        MaximizeButtonIcon.Text = WindowState == WindowState.Maximized ? "\uE923" : "\uE922";
    }

    private void ToggleWindowState()
    {
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    }
}
