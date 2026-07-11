using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using ArkheideSystem.Flourish.Abstract;
using Microsoft.Extensions.DependencyInjection;

namespace ArkheideSystem.Gallery.Views;

public partial class NavigationRuntimePage : Page
{
    private const string RuntimeGroupId = "runtime-gallery";
    private const string RuntimeItemId = "runtime-gallery.preview";
    private const string RuntimeRouteKey = "RuntimePreview";

    private readonly INavigationPanelService panel;
    private readonly INavigationMenuService menu;
    private readonly INavigationRouteRegistry routes;
    private readonly INavigationService navigation;
    private readonly IPageCacheService cache;

    public NavigationRuntimePage(
        INavigationPanelService panel,
        INavigationMenuService menu,
        INavigationRouteRegistry routes,
        INavigationService navigation,
        IPageCacheService cache
    )
    {
        this.panel = panel;
        this.menu = menu;
        this.routes = routes;
        this.navigation = navigation;
        this.cache = cache;
        InitializeComponent();

        DirectionBox.ItemsSource = Enum.GetValues<NavigationPanelDirection>();
        Loaded += Page_Loaded;
        Unloaded += Page_Unloaded;
        RefreshState();
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        panel.Changed -= RuntimeState_Changed;
        menu.Changed -= RuntimeState_Changed;
        routes.Changed -= RuntimeState_Changed;
        cache.Changed -= RuntimeState_Changed;
        panel.Changed += RuntimeState_Changed;
        menu.Changed += RuntimeState_Changed;
        routes.Changed += RuntimeState_Changed;
        cache.Changed += RuntimeState_Changed;
        RefreshState();
    }

    private void Page_Unloaded(object sender, RoutedEventArgs e)
    {
        panel.Changed -= RuntimeState_Changed;
        menu.Changed -= RuntimeState_Changed;
        routes.Changed -= RuntimeState_Changed;
        cache.Changed -= RuntimeState_Changed;
    }

    private void TogglePanel_Click(object sender, RoutedEventArgs e) => panel.Toggle();

    private void TogglePanelEnabled_Click(object sender, RoutedEventArgs e) =>
        panel.SetEnabled(!panel.Current.IsEnabled);

    private void DirectionBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (IsLoaded && DirectionBox.SelectedItem is NavigationPanelDirection direction)
        {
            panel.SetDirection(direction);
        }
    }

    private void ApplyWidths_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            panel.SetPanelWidth(
                Parse(OpenWidthBox.Text),
                Parse(ClosedWidthBox.Text),
                Parse(MaxWidthBox.Text),
                Parse(MinWidthBox.Text)
            );
        }
        catch (Exception error)
        {
            PanelStateText.Text = error.Message;
        }
    }

    private void InstallRoute_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            routes.Upsert(
                new FlourishNavigationRoute(
                    RuntimeRouteKey,
                    typeof(RuntimeRoutePage),
                    FlourishPageCacheMode.Enabled,
                    static provider =>
                        new RuntimeRoutePage(
                            provider.GetRequiredService<INavigationService>()
                        )
                )
            );

            var hasGroup = menu.Current.Groups.Any(group => group.Id == RuntimeGroupId);
            menu.Update(editor =>
            {
                if (!hasGroup)
                {
                    editor.AddGroup(RuntimeGroupId, "Added at runtime");
                }

                editor.UpsertItem(
                    RuntimeGroupId,
                    FlourishNavigationMenuItem.Page(
                        RuntimeItemId,
                        RuntimeRouteKey,
                        "Runtime route instance",
                        "\uE8A7"
                    )
                );
            });
        }
        catch (Exception error)
        {
            RouteStateText.Text = error.Message;
        }
    }

    private void NavigateRoute_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            navigation.Navigate(RuntimeRouteKey, DateTimeOffset.Now);
        }
        catch (Exception error)
        {
            RouteStateText.Text = error.Message;
        }
    }

    private void ToggleMenuItem_Click(object sender, RoutedEventArgs e)
    {
        var item = menu.Current.Groups.SelectMany(group => group.Items)
            .FirstOrDefault(candidate => candidate.Id == RuntimeItemId);
        if (item is null)
        {
            RouteStateText.Text = "Install the demo route first.";
            return;
        }

        menu.Update(editor => editor.SetItemEnabled(RuntimeItemId, !item.IsEnabled));
    }

    private void RemoveRoute_Click(object sender, RoutedEventArgs e)
    {
        menu.Update(editor =>
        {
            editor.RemoveItem(RuntimeItemId);
            if (menu.Current.Groups.Any(group => group.Id == RuntimeGroupId))
            {
                editor.RemoveGroup(RuntimeGroupId);
            }
        });
        routes.Remove(RuntimeRouteKey);
    }

    private void EnableCache_Click(object sender, RoutedEventArgs e) =>
        SetCacheMode(FlourishPageCacheMode.Enabled);

    private void DisableCache_Click(object sender, RoutedEventArgs e) =>
        SetCacheMode(FlourishPageCacheMode.Disabled);

    private void EvictCache_Click(object sender, RoutedEventArgs e)
    {
        cache.Evict(typeof(RuntimeRoutePage));
        RefreshState();
    }

    private void ClearCache_Click(object sender, RoutedEventArgs e)
    {
        cache.Clear();
        RefreshState();
    }

    private void SetCacheMode(FlourishPageCacheMode mode)
    {
        try
        {
            if (!routes.Contains(RuntimeRouteKey))
            {
                InstallRoute_Click(this, new RoutedEventArgs());
            }

            routes.SetCacheMode(RuntimeRouteKey, mode);
            cache.SetCacheMode(typeof(RuntimeRoutePage), mode);
        }
        catch (Exception error)
        {
            CacheStateText.Text = error.Message;
        }
    }

    private void RuntimeState_Changed(object? sender, EventArgs e) =>
        Dispatcher.BeginInvoke(RefreshState);

    private void RefreshState()
    {
        var panelState = panel.Current;
        DirectionBox.SelectedItem = panelState.Direction;
        PanelStateText.Text =
            $"Enabled: {panelState.IsEnabled}  |  Open: {panelState.IsOpen}  |  Side: {panelState.Direction}  |  Widths: {panelState.ClosedWidth:0}-{panelState.OpenWidth:0} ({panelState.MinWidth:0}-{panelState.MaxWidth:0})";

        var routeInstalled = routes.Contains(RuntimeRouteKey);
        var menuItem = menu.Current.Groups.SelectMany(group => group.Items)
            .FirstOrDefault(item => item.Id == RuntimeItemId);
        RouteStateText.Text =
            $"Route installed: {routeInstalled}  |  Menu item: {(menuItem is null ? "missing" : menuItem.IsEnabled ? "enabled" : "disabled")}  |  Current route: {navigation.CurrentNavigationKey ?? "<none>"}";

        var cacheState = cache.Current;
        var configuredMode = cacheState.CacheModes.GetValueOrDefault(
            typeof(RuntimeRoutePage),
            FlourishPageCacheMode.Disabled
        );
        CacheStateText.Text =
            $"Mode: {configuredMode}  |  Instance cached: {cacheState.CachedPageTypes.Contains(typeof(RuntimeRoutePage))}  |  Total cached types: {cacheState.CachedPageTypes.Count}";
    }

    private static double Parse(string value) =>
        double.Parse(value, NumberStyles.Float, CultureInfo.InvariantCulture);
}
