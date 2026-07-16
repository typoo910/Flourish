using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Controls;

namespace ArkheideSystem.Gallery.Views;

public partial class TitleBarRuntimePage : Page
{
    private readonly ITitleBarService titleBar;
    private readonly ITitleBarSearchService search;
    private readonly IShellFeatureService shellFeatures;
    private IDisposable? searchSubscription;
    private bool isRefreshing;

    public TitleBarRuntimePage(
        ITitleBarService titleBar,
        ITitleBarSearchService search,
        IShellFeatureService shellFeatures
    )
    {
        this.titleBar = titleBar;
        this.search = search;
        this.shellFeatures = shellFeatures;
        InitializeComponent();

        TitleBarElementBox.ItemsSource = Enum.GetValues<TitleBarElement>();
        BreadcrumbModeBox.ItemsSource = Enum.GetValues<BreadcrumbShowOption>();
        ShellFeatureBox.ItemsSource = Enum.GetValues<ShellFeature>();
        TitleBarElementBox.SelectedItem = TitleBarElement.Search;
        ShellFeatureBox.SelectedItem = ShellFeature.TitleBar;

        Loaded += Page_Loaded;
        Unloaded += Page_Unloaded;
        RefreshState();
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        Page_Unloaded(sender, e);
        titleBar.Changed += TitleBar_Changed;
        search.StateChanged += Search_StateChanged;
        shellFeatures.Changed += ShellFeatures_Changed;
        searchSubscription = search.Subscribe(HandleSearchQueryAsync);
        RefreshState();
    }

    private void Page_Unloaded(object sender, RoutedEventArgs e)
    {
        titleBar.Changed -= TitleBar_Changed;
        search.StateChanged -= Search_StateChanged;
        shellFeatures.Changed -= ShellFeatures_Changed;
        searchSubscription?.Dispose();
        searchSubscription = null;
    }

    private void TitleBar_Changed(object? sender, FlourishTitleBarChangedEventArgs e)
    {
        Dispatcher.BeginInvoke(RefreshState);
    }

    private void Search_StateChanged(object? sender, FlourishTitleBarSearchStateChangedEventArgs e)
    {
        Dispatcher.BeginInvoke(RefreshSearchState);
    }

    private void ShellFeatures_Changed(object? sender, FlourishShellFeatureChangedEventArgs e)
    {
        Dispatcher.BeginInvoke(RefreshShellFeatureState);
    }

    private async ValueTask HandleSearchQueryAsync(
        FlourishTitleBarSearchChangedEventArgs args,
        CancellationToken cancellationToken
    )
    {
        await Task.Delay(250, cancellationToken);
        await Dispatcher.InvokeAsync(() =>
        {
            SearchStatusText.Text = string.IsNullOrWhiteSpace(args.Text)
                ? $"Query #{args.Sequence}: empty query"
                : $"Query #{args.Sequence}: simulated results for \"{args.Text}\" completed at {DateTime.Now:T}.";
        });
    }

    private void ApplyIdentity_Click(object sender, RoutedEventArgs e)
    {
        Execute(
            () => titleBar.SetIdentity(TitleBox.Text, NullIfWhiteSpace(SubtitleBox.Text)),
            IdentityStatusText
        );
    }

    private void IdentityBox_LostFocus(object sender, RoutedEventArgs e) => CommitIdentity();

    private void IdentityBox_KeyDown(object sender, KeyEventArgs e) =>
        CommitOnEnter(e, CommitIdentity);

    private void CommitIdentity()
    {
        if (CanApplyImmediately)
        {
            ApplyIdentity_Click(this, new RoutedEventArgs());
        }
    }

    private void ApplyLogo_Click(object sender, RoutedEventArgs e)
    {
        Execute(
            () =>
                titleBar.SetLogo(
                    NullIfWhiteSpace(LogoPathBox.Text),
                    NullIfWhiteSpace(LogoFallbackBox.Text)
                ),
            IdentityStatusText
        );
    }

    private void LogoBox_LostFocus(object sender, RoutedEventArgs e) => CommitLogo();

    private void LogoBox_KeyDown(object sender, KeyEventArgs e) => CommitOnEnter(e, CommitLogo);

    private void CommitLogo()
    {
        if (CanApplyImmediately)
        {
            ApplyLogo_Click(this, new RoutedEventArgs());
        }
    }

    private void TitleBarElementBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        RefreshSelectedElementState();
    }

    private void ApplyElementVisibility_Click(object sender, RoutedEventArgs e)
    {
        if (TitleBarElementBox.SelectedItem is TitleBarElement element)
        {
            Execute(
                () => titleBar.SetElementVisible(element, TitleBarElementVisibleBox.IsChecked == true),
                IdentityStatusText
            );
        }
    }

    private void TitleBarElementVisibleBox_Changed(object sender, RoutedEventArgs e)
    {
        if (CanApplyImmediately)
        {
            ApplyElementVisibility_Click(sender, new RoutedEventArgs());
        }
    }

    private void ApplyBreadcrumbMode_Click(object sender, RoutedEventArgs e)
    {
        if (BreadcrumbModeBox.SelectedItem is BreadcrumbShowOption mode)
        {
            Execute(() => titleBar.SetBreadcrumbMode(mode), IdentityStatusText);
        }
    }

    private void BreadcrumbModeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CanApplyImmediately)
        {
            ApplyBreadcrumbMode_Click(sender, new RoutedEventArgs());
        }
    }

    private void SetSearchText_Click(object sender, RoutedEventArgs e)
    {
        Execute(() => search.SetText(SearchTextBox.Text), SearchStatusText);
    }

    private void SearchTextBox_LostFocus(object sender, RoutedEventArgs e) => CommitSearchText();

    private void SearchTextBox_KeyDown(object sender, KeyEventArgs e) =>
        CommitOnEnter(e, CommitSearchText);

    private void CommitSearchText()
    {
        if (CanApplyImmediately)
        {
            SetSearchText_Click(this, new RoutedEventArgs());
        }
    }

    private void FocusSearch_Click(object sender, RoutedEventArgs e)
    {
        Execute(search.Focus, SearchStatusText);
    }

    private void ClearSearch_Click(object sender, RoutedEventArgs e)
    {
        Execute(search.Clear, SearchStatusText);
    }

    private void ApplySearchPlaceholder_Click(object sender, RoutedEventArgs e)
    {
        Execute(() => search.SetPlaceholder(SearchPlaceholderBox.Text), SearchStatusText);
    }

    private void SearchPlaceholderBox_LostFocus(object sender, RoutedEventArgs e) =>
        CommitSearchPlaceholder();

    private void SearchPlaceholderBox_KeyDown(object sender, KeyEventArgs e) =>
        CommitOnEnter(e, CommitSearchPlaceholder);

    private void CommitSearchPlaceholder()
    {
        if (CanApplyImmediately)
        {
            ApplySearchPlaceholder_Click(this, new RoutedEventArgs());
        }
    }

    private void ToggleSearchVisibility_Click(object sender, RoutedEventArgs e)
    {
        Execute(() => search.SetVisible(!search.Current.IsVisible), SearchStatusText);
    }

    private void ShellFeatureBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        RefreshShellFeatureState();
    }

    private void ApplyShellFeature_Click(object sender, RoutedEventArgs e)
    {
        if (ShellFeatureBox.SelectedItem is ShellFeature feature)
        {
            Execute(
                () => shellFeatures.SetEnabled(feature, ShellFeatureEnabledBox.IsChecked == true),
                ShellFeatureStatusText
            );
        }
    }

    private void ShellFeatureEnabledBox_Changed(object sender, RoutedEventArgs e)
    {
        if (CanApplyImmediately)
        {
            ApplyShellFeature_Click(sender, new RoutedEventArgs());
        }
    }

    private bool CanApplyImmediately => IsLoaded && !isRefreshing;

    private static void CommitOnEnter(KeyEventArgs e, Action commit)
    {
        if (e.Key != Key.Enter)
        {
            return;
        }

        commit();
        e.Handled = true;
    }

    private void Execute(Action action, FlourishTextBlock status)
    {
        try
        {
            action();
            RefreshState();
        }
        catch (Exception error)
        {
            status.Text = error.Message;
        }
    }

    private void RefreshState()
    {
        isRefreshing = true;
        try
        {
            var current = titleBar.Current;
            TitleBox.Text = current.Title;
            SubtitleBox.Text = current.Subtitle;
            LogoPathBox.Text = current.LogoPath ?? string.Empty;
            LogoFallbackBox.Text = current.LogoFallbackText;
            BreadcrumbModeBox.SelectedItem = current.BreadcrumbMode;
            IdentityStatusText.Text =
                $"Title: {current.Title}  |  Subtitle visible: {current.IsSubtitleVisible}  |  Logo visible: {current.IsLogoVisible}";
            RefreshSelectedElementState();
            RefreshSearchState();
            RefreshShellFeatureState();
        }
        finally
        {
            isRefreshing = false;
        }
    }

    private void RefreshSearchState()
    {
        var current = search.Current;
        SearchTextBox.Text = current.Text;
        SearchPlaceholderBox.Text = current.Placeholder;
        ToggleSearchVisibilityButton.Content = current.IsVisible ? "Hide search" : "Show search";
    }

    private void RefreshSelectedElementState()
    {
        if (TitleBarElementBox.SelectedItem is TitleBarElement element)
        {
            var wasRefreshing = isRefreshing;
            isRefreshing = true;
            try
            {
                TitleBarElementVisibleBox.IsChecked = IsTitleBarElementVisible(
                    titleBar.Current,
                    element
                );
            }
            finally
            {
                isRefreshing = wasRefreshing;
            }
        }
    }

    private void RefreshShellFeatureState()
    {
        if (ShellFeatureBox.SelectedItem is ShellFeature feature)
        {
            var wasRefreshing = isRefreshing;
            isRefreshing = true;
            try
            {
                var enabled = shellFeatures.Current.IsEnabled(feature);
                ShellFeatureEnabledBox.IsChecked = enabled;
                ShellFeatureStatusText.Text =
                    $"{feature}: {(enabled ? "enabled" : "disabled")}  |  State version: {shellFeatures.Current.Version}";
            }
            finally
            {
                isRefreshing = wasRefreshing;
            }
        }
    }

    private static bool IsTitleBarElementVisible(
        FlourishTitleBarState state,
        TitleBarElement element
    ) =>
        element switch
        {
            TitleBarElement.Search => state.IsSearchVisible,
            TitleBarElement.Breadcrumb => state.IsBreadcrumbVisible,
            TitleBarElement.NavigationToggle => state.IsNavigationToggleVisible,
            TitleBarElement.Logo => state.IsLogoVisible,
            TitleBarElement.Title => state.IsTitleVisible,
            TitleBarElement.Subtitle => state.IsSubtitleVisible,
            TitleBarElement.ThemeToggle => state.IsThemeToggleVisible,
            TitleBarElement.Profile => state.IsProfileVisible,
            _ => false,
        };

    private static string? NullIfWhiteSpace(string value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
