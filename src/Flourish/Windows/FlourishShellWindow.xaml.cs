using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using AcksheedSys.Flourish.Abstract;
using AcksheedSys.Flourish.Models;
using AcksheedSys.Flourish.Services;

namespace AcksheedSys.Flourish.Windows;

internal partial class FlourishShellWindow : Window
{
    private readonly INavigationService navigationService;
    private readonly IFrameNavigationService frameNavigationService;
    private readonly IFlourishToolbarService toolbarService;
    private readonly IFlourishStatusService statusService;
    private readonly FlourishShellOptions options;
    private readonly Dictionary<string, FlourishNavigationItem> navigationItemsByKey = new(
        StringComparer.Ordinal
    );
    private readonly Dictionary<Type, FlourishNavigationItem> navigationItemsByPage = [];
    private readonly Dictionary<Type, IReadOnlyList<Button>> toolbarButtonsByPageType = [];
    private IReadOnlyList<Button>? defaultToolbarButtons;
    private FlourishNavigationItem? firstNavigationItem;
    private Style toolbarButtonStyle = null!;
    private Style navigationListBoxItemStyle = null!;
    private Brush mutedTextBrush = null!;
    private FontFamily iconFontFamily = null!;
    private Type? activeToolbarPageType;
    private bool isDefaultToolbarActive;
    private bool isPaneOpen = true;
    private bool suppressNavigationSelection;

    public FlourishShellWindow(
        INavigationService navigationService,
        IFrameNavigationService frameNavigationService,
        IFlourishToolbarService toolbarService,
        IFlourishStatusService statusService,
        FlourishShellOptions options
    )
    {
        InitializeComponent();

        this.navigationService = navigationService;
        this.frameNavigationService = frameNavigationService;
        this.toolbarService = toolbarService;
        this.statusService = statusService;
        this.options = options;

        CacheResources();
        ApplyOptions();
        BuildToolbarItems();
        BuildNavigationItems();
        BuildStatusItems();

        StateChanged += MainWindow_StateChanged;
        frameNavigationService.Initialize(RootFrame);
        navigationService.Navigated += RootFrame_Navigated;

        NavigateToInitialPage();
    }

    private void CacheResources()
    {
        toolbarButtonStyle = (Style)FindResource("FlourishToolbarButtonStyle");
        navigationListBoxItemStyle = (Style)FindResource("FlourishNavigationListBoxItemStyle");
        mutedTextBrush = (Brush)FindResource("MutedTextBrush");
        iconFontFamily = new FontFamily("Segoe MDL2 Assets");
    }

    private void ApplyOptions()
    {
        Title = options.Title;
        AppTitleText.Text = options.Title;
        AppSubtitleText.Text = options.Subtitle;
        PaneTitle.Text = options.PaneTitle;
        SearchBox.Text = options.SearchPlaceholder;
        StatusTextBlock.Text = statusService.StatusText;
        SearchBoxHost.Visibility = options.IsTitlebarSearchEnabled
            ? Visibility.Visible
            : Visibility.Collapsed;
        NavigationPaneBorder.Visibility = options.IsNavigationPanelEnabled
            ? Visibility.Visible
            : Visibility.Collapsed;
        PaneToggleButton.Visibility = options.IsNavigationPanelEnabled
            ? Visibility.Visible
            : Visibility.Collapsed;
        BreadcrumbHost.Visibility = options.IsBreadcrumbEnabled
            ? Visibility.Visible
            : Visibility.Collapsed;

        ApplyNavigationPanelPlacement();
        SetPaneWidth(options.IsNavigationPanelEnabled ? options.OpenPaneWidth : 0);

        if (options.LogoSource is not null)
        {
            AppLogoImage.Source = options.LogoSource;
            AppLogoImage.Visibility = Visibility.Visible;
            AppLogoFallback.Visibility = Visibility.Collapsed;
            return;
        }

        AppLogoFallback.Text = string.IsNullOrWhiteSpace(options.LogoFallbackText)
            ? "F"
            : options.LogoFallbackText[..1];
        AppLogoFallback.Visibility = Visibility.Visible;
    }

    private void ApplyNavigationPanelPlacement()
    {
        if (options.NavigationPanelDirection == NavigationPanelDirection.Right)
        {
            Grid.SetColumn(RootFrame, 0);
            Grid.SetColumn(NavigationPaneBorder, 1);
            NavigationPaneBorder.BorderThickness = new Thickness(1, 0, 0, 0);
            return;
        }

        Grid.SetColumn(NavigationPaneBorder, 0);
        Grid.SetColumn(RootFrame, 1);
        NavigationPaneBorder.BorderThickness = new Thickness(0, 0, 1, 0);
    }

    private void SetPaneWidth(double width)
    {
        if (options.NavigationPanelDirection == NavigationPanelDirection.Right)
        {
            PaneColumn.Width = new GridLength(1, GridUnitType.Star);
            ContentColumn.Width = new GridLength(width);
            return;
        }

        PaneColumn.Width = new GridLength(width);
        ContentColumn.Width = new GridLength(1, GridUnitType.Star);
    }

    private void BuildToolbarItems(Type? pageType = null)
    {
        if (activeToolbarPageType == pageType && isDefaultToolbarActive == (pageType is null))
        {
            return;
        }

        ToolbarItemsHost.Children.Clear();
        var toolbarButtons = GetToolbarButtons(pageType);
        ToolbarHostBorder.Visibility =
            toolbarButtons.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

        foreach (var button in toolbarButtons)
        {
            ToolbarItemsHost.Children.Add(button);
        }

        activeToolbarPageType = pageType;
        isDefaultToolbarActive = pageType is null;
    }

    private IReadOnlyList<Button> GetToolbarButtons(Type? pageType)
    {
        if (pageType is null)
        {
            return defaultToolbarButtons ??= CreateToolbarButtons(toolbarService.GetToolbarItems());
        }

        if (!toolbarButtonsByPageType.TryGetValue(pageType, out var toolbarButtons))
        {
            toolbarButtons = CreateToolbarButtons(toolbarService.GetToolbarItems(pageType));
            toolbarButtonsByPageType[pageType] = toolbarButtons;
        }

        return toolbarButtons;
    }

    private IReadOnlyList<Button> CreateToolbarButtons(IReadOnlyList<FlourishToolbarItem> items)
    {
        var buttons = new List<Button>(items.Count);
        foreach (var item in items)
        {
            buttons.Add(
                new Button
                {
                    Content = CreateIconTextContent(item.IconGlyph, item.DisplayName),
                    Margin = buttons.Count > 0 ? new Thickness(6, 0, 0, 0) : new Thickness(),
                    Style = toolbarButtonStyle,
                    Tag = item.CommandKey,
                }
            );
        }

        return buttons;
    }

    private void BuildNavigationItems()
    {
        navigationItemsByKey.Clear();
        navigationItemsByPage.Clear();
        firstNavigationItem = null;

        foreach (var item in options.NavigationItems)
        {
            item.Validate();
            firstNavigationItem ??= item;
            navigationItemsByKey[item.Key] = item;
            navigationItemsByPage[item.PageType] = item;
        }

        NavigationItemsHost.ItemContainerStyle = navigationListBoxItemStyle;
        NavigationItemsHost.ItemsSource = options.NavigationItems;
    }

    private void BuildStatusItems()
    {
        StatusItemsHost.Children.Clear();

        foreach (var item in statusService.StatusItems)
        {
            var status = new StackPanel
            {
                Margin =
                    StatusItemsHost.Children.Count > 0
                        ? new Thickness(20, 0, 0, 0)
                        : new Thickness(),
                Orientation = Orientation.Horizontal,
            };
            status.Children.Add(
                new TextBlock
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    FontFamily = iconFontFamily,
                    Foreground = mutedTextBrush,
                    Text = item.IconGlyph,
                }
            );
            status.Children.Add(
                new TextBlock
                {
                    Margin = new Thickness(7, 0, 0, 0),
                    VerticalAlignment = VerticalAlignment.Center,
                    Text = item.Text,
                }
            );

            StatusItemsHost.Children.Add(status);
        }
    }

    private StackPanel CreateIconTextContent(string iconGlyph, string label)
    {
        var content = new StackPanel { Orientation = Orientation.Horizontal };
        content.Children.Add(
            new TextBlock
            {
                VerticalAlignment = VerticalAlignment.Center,
                FontFamily = iconFontFamily,
                Text = iconGlyph,
            }
        );
        content.Children.Add(
            new TextBlock
            {
                Margin = new Thickness(7, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center,
                Text = label,
            }
        );
        return content;
    }

    private void NavigateToInitialPage()
    {
        var initialItem =
            options.InitialNavigationPageType is null
                ? GetNavigationItem(options.InitialNavigationKey) ?? firstNavigationItem
                : navigationItemsByPage.GetValueOrDefault(options.InitialNavigationPageType);

        if (initialItem is null)
        {
            return;
        }

        SelectNavigationItem(initialItem);
        NavigateToKey(initialItem.Key, false);
    }

    private FlourishNavigationItem? GetNavigationItem(string? key)
    {
        return key is not null && navigationItemsByKey.TryGetValue(key, out var item) ? item : null;
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
        SetPaneWidth(isPaneOpen ? options.OpenPaneWidth : options.ClosedPaneWidth);
        PaneTitle.Visibility = isPaneOpen ? Visibility.Visible : Visibility.Collapsed;
        NavigationItemsHost.Tag = isPaneOpen;
    }

    private void NavigationItemsHost_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (
            suppressNavigationSelection
            || NavigationItemsHost.SelectedItem is not FlourishNavigationItem item
        )
        {
            return;
        }

        NavigateToKey(item.Key, true);
    }

    private void RootFrame_Navigated(object? sender, FlourishNavigatedEventArgs e)
    {
        BackButton.IsEnabled = navigationService.CanGoBack;
        BuildToolbarItems(e.SourcePageType);
        UpdateBreadcrumb(e.SourcePageType);

        SelectNavigationItem(e.SourcePageType);
    }

    private void UpdateBreadcrumb(Type sourcePageType)
    {
        if (
            !options.IsBreadcrumbEnabled
            || options.BreadcrumbShowOption == BreadcrumbShowOption.Hidden
        )
        {
            BreadcrumbHost.Visibility = Visibility.Collapsed;
            return;
        }

        if (
            options.BreadcrumbShowOption == BreadcrumbShowOption.OnlyAvailable
            && !navigationService.CanGoBack
        )
        {
            BreadcrumbHost.Visibility = Visibility.Collapsed;
            return;
        }

        var label =
            navigationItemsByPage.GetValueOrDefault(sourcePageType)?.Label
            ?? sourcePageType.Name;

        BreadcrumbText.Text = $"{options.Title} / {label}";
        BreadcrumbHost.Visibility = Visibility.Visible;
    }

    private void NavigateToKey(string key, bool addToBackStack)
    {
        if (!navigationItemsByKey.TryGetValue(key, out var targetItem))
        {
            targetItem = firstNavigationItem;
        }

        if (targetItem is null || navigationService.CurrentSourcePageType == targetItem.PageType)
        {
            return;
        }

        navigationService.Navigate(targetItem.PageType, addToBackStack: addToBackStack);

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
        WindowState =
            WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    }

    private void SelectNavigationItem(Type pageType)
    {
        if (navigationItemsByPage.TryGetValue(pageType, out var item))
        {
            SelectNavigationItem(item);
        }
    }

    private void SelectNavigationItem(FlourishNavigationItem item)
    {
        suppressNavigationSelection = true;
        try
        {
            NavigationItemsHost.SelectedItem = item;
        }
        finally
        {
            suppressNavigationSelection = false;
        }
    }
}
