using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shell;
using System.Windows.Threading;
using System.ComponentModel;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Configuration;
using ArkheideSystem.Flourish.Controls;
using ArkheideSystem.Flourish.Services;
using Microsoft.Extensions.DependencyInjection;
using Button = System.Windows.Controls.Button;
using FontFamily = System.Windows.Media.FontFamily;
using ListBox = System.Windows.Controls.ListBox;
using Orientation = System.Windows.Controls.Orientation;
using WpfPanel = System.Windows.Controls.Panel;

namespace ArkheideSystem.Flourish.Windows;

internal partial class FlourishShellWindow : Window
{
    private readonly INavigationService navigationService;
    private readonly IFrameNavigationService frameNavigationService;
    private readonly FlourishToolbarService toolbarService;
    private readonly FlourishStatusService statusService;
    private readonly IBackgroundTaskService backgroundTaskService;
    private readonly IMessageService messageService;
    private readonly TrayIconService trayIconService;
    private readonly CommandParser commandParser;
    private readonly FontService fontService;
    private readonly MaterialEffectService materialEffectService;
    private readonly ThemeService themeService;
    private readonly FlourishMotionService motionService;
    private readonly WindowFrameFixService windowFrameFixService;
    private readonly IProfileService profileService;
    private readonly FlourishLocalizationService localizationService;
    private readonly IServiceProvider serviceProvider;
    private readonly FlourishShellOptions options;
    private readonly Dictionary<string, FlourishNavigationItem> navigationItemsByKey = new(
        StringComparer.Ordinal
    );
    private readonly Dictionary<Type, FlourishNavigationItem> navigationItemsByPage = [];
    private readonly Dictionary<NavigationTreeKey, FlourishNavigationItem> navigationParentsByKey =
        [];
    private readonly Dictionary<
        NavigationTreeKey,
        List<FlourishNavigationItem>
    > navigationChildrenByParentKey = [];
    private readonly Dictionary<Type, IReadOnlyList<Button>> toolbarButtonsByPageType = [];
    private readonly Dictionary<Guid, BackgroundTaskIconView> backgroundTaskIconsById = [];
    private readonly Dictionary<Guid, BackgroundTaskRowView> backgroundTaskRowsById = [];
    private readonly object backgroundTaskRefreshGate = new();
    private readonly DispatcherTimer backgroundTaskRefreshTimer;
    private IReadOnlyList<Button>? defaultToolbarButtons;
    private FlourishNavigationItem? firstNavigationItem;
    private Style toolbarButtonStyle = null!;
    private Style navigationListBoxItemStyle = null!;
    private FontFamily iconFontFamily = null!;
    private Type? activeToolbarPageType;
    private FlourishNavigationItem? selectedNavigationItem;
    private FlourishNavigationItem? activeChildParentItem;
    private bool isDefaultToolbarActive;
    private bool isPaneOpen = true;
    private bool suppressNavigationSelection;
    private double navigationPaneDragStartWidth;
    private FrameworkElement? statusFlyoutAnchor;
    private Guid? statusFlyoutAnchorTaskId;
    private StatusFlyoutKind statusFlyoutKind;
    private IReadOnlyList<FlourishBackgroundTaskInfo> backgroundTasks = [];
    private IReadOnlyList<FlourishBackgroundTaskInfo> pendingBackgroundTasks = [];
    private TextBlock? backgroundTaskEmptyText;
    private IInputElement? statusFlyoutRestoreFocusTarget;
    private bool backgroundTaskRefreshPending;
    private bool backgroundTaskRefreshLoopActive;
    private bool isShellClosed;
    private bool statusFlyoutOpenedWithFocus;

    private readonly record struct NavigationTreeKey(bool IsFixed, int GroupId, int RelationshipId);

    private enum StatusFlyoutKind
    {
        None,
        BackgroundTasks,
        System,
    }

    private sealed class BackgroundTaskIconView(
        Button button,
        TextBlock icon,
        TextBlock toolTipName,
        TextBlock toolTipDescription,
        TextBlock toolTipState
    )
    {
        public Button Button { get; } = button;

        public TextBlock Icon { get; } = icon;

        public TextBlock ToolTipName { get; } = toolTipName;

        public TextBlock ToolTipDescription { get; } = toolTipDescription;

        public TextBlock ToolTipState { get; } = toolTipState;
    }

    private sealed class BackgroundTaskRowView(
        Border container,
        TextBlock icon,
        TextBlock name,
        TextBlock description,
        TextBlock state,
        Button cancelButton
    )
    {
        public Border Container { get; } = container;

        public TextBlock Icon { get; } = icon;

        public TextBlock Name { get; } = name;

        public TextBlock Description { get; } = description;

        public TextBlock State { get; } = state;

        public Button CancelButton { get; } = cancelButton;
    }

    public FlourishShellWindow(
        INavigationService navigationService,
        IFrameNavigationService frameNavigationService,
        FlourishToolbarService toolbarService,
        FlourishStatusService statusService,
        IBackgroundTaskService backgroundTaskService,
        IMessageService messageService,
        TrayIconService trayIconService,
        CommandParser commandParser,
        FontService fontService,
        MaterialEffectService materialEffectService,
        ThemeService themeService,
        FlourishMotionService motionService,
        WindowFrameFixService windowFrameFixService,
        IProfileService profileService,
        FlourishLocalizationService localizationService,
        IServiceProvider serviceProvider,
        FlourishShellOptions options
    )
    {
        themeService.Initialize(System.Windows.Application.Current);
        InitializeComponent();
        backgroundTaskRefreshTimer = new DispatcherTimer(
            DispatcherPriority.Background,
            Dispatcher
        )
        {
            Interval = TimeSpan.FromMilliseconds(33),
        };
        backgroundTaskRefreshTimer.Tick += BackgroundTaskRefreshTimer_Tick;

        this.navigationService = navigationService;
        this.frameNavigationService = frameNavigationService;
        this.toolbarService = toolbarService;
        this.statusService = statusService;
        this.backgroundTaskService = backgroundTaskService;
        this.messageService = messageService;
        this.trayIconService = trayIconService;
        this.commandParser = commandParser;
        this.fontService = fontService;
        this.materialEffectService = materialEffectService;
        this.themeService = themeService;
        this.motionService = motionService;
        this.windowFrameFixService = windowFrameFixService;
        this.profileService = profileService;
        this.localizationService = localizationService;
        this.serviceProvider = serviceProvider;
        this.options = options;

        Titlebar.ApplyLocale(localizationService);
        fontService.Apply(this);
        CacheResources();
        ApplyOptions();
        BuildRegionContents();
        ConfigureProfileSurface();
        BuildToolbarItems();
        BuildNavigationItems();
        BuildStatusItems();
        backgroundTaskService.TasksChanged += BackgroundTaskService_TasksChanged;
        RefreshBackgroundTaskStatus(backgroundTaskService.ActiveTasks);
        AttachTitlebarEvents();

        themeService.ThemeChanged += ThemeService_ThemeChanged;
        StateChanged += MainWindow_StateChanged;
        Closing += ShellWindow_Closing;
        Closed += ShellWindow_Closed;
        Loaded += ShellWindow_Loaded;
        PreviewKeyDown += ShellWindow_PreviewKeyDown;
        frameNavigationService.Initialize(RootFrame);
        navigationService.Navigated += RootFrame_Navigated;

        NavigateToInitialPage();
    }

    private void CacheResources()
    {
        toolbarButtonStyle = (Style)FindResource("FlourishToolbarButtonStyle");
        navigationListBoxItemStyle = (Style)FindResource("FlourishNavigationListBoxItemStyle");
        iconFontFamily = (FontFamily)FindResource("FlourishIconFontFamily");
    }

    private void ApplyOptions()
    {
        ApplyWindowOptions();
        Title = options.Title;
        var effectiveLogoSource = Titlebar.SetLogo(
            options.LogoPath,
            options.LogoFallbackText
        );
        Icon = options.IsTitlebarLogoEnabled ? effectiveLogoSource : null;
        Titlebar.SetTitle(options.Title);
        Titlebar.SetSubtitle(options.Subtitle);
        Titlebar.SetSearchPlaceholder(options.SearchPlaceholder);
        Titlebar.ConfigureVisibility(
            options.IsTitlebarSearchEnabled,
            IsBreadcrumbFeatureEnabled(),
            options.IsTitlebarNavigationToggleEnabled && options.IsNavigationPanelEnabled,
            options.IsTitlebarLogoEnabled,
            options.IsTitlebarTitleEnabled,
            options.IsTitlebarSubtitleEnabled,
            options.IsTitlebarThemeToggleEnabled && options.IsThemeEnabled,
            options.IsProfileEnabled && options.IsTitlebarProfileEnabled
        );
        UpdateStatusBarVisibility();
        AutomationProperties.SetName(
            SystemStatusButton,
            localizationService.Get(FlourishLocaleKeys.SystemStatusTitle)
        );
        NavigationPaneBorder.Visibility = options.IsNavigationPanelEnabled
            ? Visibility.Visible
            : Visibility.Collapsed;
        BreadcrumbHost.Visibility = IsBreadcrumbFeatureEnabled()
            ? Visibility.Visible
            : Visibility.Collapsed;
        UpdateTitlebarBreadcrumbNavigation();
        ApplyMotionResources();
        ApplyToolTipResources();

        NormalizeNavigationPaneWidths();
        ApplyNavigationPanelPlacement();
        isPaneOpen = options.IsNavigationPanelInitiallyOpen;
        ApplyNavigationPaneState();
        windowFrameFixService.Attach(this);
        materialEffectService.Attach(
            this,
            options.IsMaterialEffectEnabled ? options.MaterialEffect : MaterialEffect.None
        );
        themeService.Attach(this);
        ApplyThemeState();
        trayIconService.Initialize(this, options.Title);
    }

    private void ConfigureProfileSurface()
    {
        if (
            !options.IsTitlebarEnabled
            || !options.IsProfileEnabled
            || !options.IsTitlebarProfileEnabled
        )
        {
            return;
        }

        var profilePage = ActivatorUtilities.GetServiceOrCreateInstance(
            serviceProvider,
            options.Profile.PageType
        );
        if (profilePage is not Page page)
        {
            throw new InvalidOperationException(
                $"Configured profile page {options.Profile.PageType.FullName} is not a WPF Page."
            );
        }

        Titlebar.SetProfile(profileService.CurrentProfile);
        ProfileFrame.Navigate(page);
        profileService.ProfileChanged += ProfileService_ProfileChanged;
    }

    private async void ShellWindow_Loaded(object sender, RoutedEventArgs e)
    {
        Loaded -= ShellWindow_Loaded;
        if (!options.IsProfileEnabled)
        {
            return;
        }

        try
        {
            await profileService.InitializeAsync();
            Titlebar.SetProfile(profileService.CurrentProfile);
        }
        catch (Exception error)
        {
            System.Diagnostics.Debug.WriteLine(
                $"Flourish profile initialization failed: {error}"
            );
        }
    }

    private void ProfileService_ProfileChanged(object? sender, ProfileChangedEventArgs e)
    {
        if (!Dispatcher.CheckAccess())
        {
            Dispatcher.Invoke(() => Titlebar.SetProfile(e.Profile));
            return;
        }

        Titlebar.SetProfile(e.Profile);
    }

    private void ApplyMotionResources()
    {
        Resources["FlourishHoverRevealEnabled"] =
            options.Motion.IsEnabled
            && options.Motion.IsHoverRevealEnabled
            && (
                !options.Motion.RespectSystemReducedMotion
                || SystemParameters.ClientAreaAnimation
            );
        HoverReveal.SetAnimationDuration(this, options.Motion.HoverRevealAnimationDuration);
    }

    private void ApplyToolTipResources()
    {
        Resources["FlourishToolTipInitialShowDelay"] = options.IsTipsEnabled
            ? options.Tips.InitialShowDelayMilliseconds
            : int.MaxValue;
        Resources["FlourishToolTipSpawnableMargin"] = options.IsTipsEnabled
            ? options.Tips.SpawnableMargin
            : 0d;
    }

    private void ApplyWindowOptions()
    {
        MinWidth = options.WindowMinWidth;
        MinHeight = options.WindowMinHeight;
        MaxWidth = options.WindowMaxWidth;
        MaxHeight = options.WindowMaxHeight;
        Width = options.WindowWidth;
        Height = options.WindowHeight;
        WindowStartupLocation = options.WindowStartupLocation;
        ResizeMode = options.WindowResizeMode;
        Topmost = options.WindowTopmost;
        ShowInTaskbar = options.WindowShowInTaskbar;

        if (options.WindowLeft is { } left)
        {
            Left = left;
        }

        if (options.WindowTop is { } top)
        {
            Top = top;
        }

        WindowState = options.WindowState;
        ApplyWindowChrome();
        Titlebar.SetMaximizeEnabled(ResizeMode is ResizeMode.CanResize or ResizeMode.CanResizeWithGrip);
    }

    private void ApplyWindowChrome()
    {
        if (!options.IsTitlebarEnabled)
        {
            WindowStyle = WindowStyle.SingleBorderWindow;
            WindowChrome.SetWindowChrome(this, null);
            ShellBorder.BorderThickness = new Thickness();
            Titlebar.Visibility = Visibility.Collapsed;
            return;
        }

        WindowStyle = WindowStyle.None;
        WindowChrome.SetWindowChrome(
            this,
            new WindowChrome
            {
                CaptionHeight = 0,
                CornerRadius = new CornerRadius(),
                GlassFrameThickness = new Thickness(),
                ResizeBorderThickness = new Thickness(6),
                UseAeroCaptionButtons = false,
            }
        );
        ShellBorder.BorderThickness = new Thickness(1);
        Titlebar.Visibility = Visibility.Visible;
    }

    private void AttachTitlebarEvents()
    {
        Titlebar.BackRequested += Titlebar_BackRequested;
        Titlebar.ForwardRequested += Titlebar_ForwardRequested;
        Titlebar.NavigationToggleRequested += Titlebar_NavigationToggleRequested;
        Titlebar.MinimizeRequested += Titlebar_MinimizeRequested;
        Titlebar.MaximizeRequested += Titlebar_MaximizeRequested;
        Titlebar.CloseRequested += Titlebar_CloseRequested;
        Titlebar.DragRequested += Titlebar_DragRequested;
        Titlebar.ToggleWindowStateRequested += Titlebar_ToggleWindowStateRequested;
        Titlebar.ThemeToggleRequested += Titlebar_ThemeToggleRequested;
        Titlebar.ProfileToggleRequested += Titlebar_ProfileToggleRequested;
        Titlebar.SearchTextChanged += Titlebar_SearchTextChanged;
    }

    private void Titlebar_ProfileToggleRequested(object? sender, EventArgs e)
    {
        if (ProfileOverlay.Visibility == Visibility.Visible)
        {
            CloseProfileOverlay();
            return;
        }

        if (
            !options.IsTitlebarEnabled
            || !options.IsProfileEnabled
            || !options.IsTitlebarProfileEnabled
        )
        {
            return;
        }

        CloseStatusFlyout(restoreFocus: false);
        ProfileOverlay.Visibility = Visibility.Visible;
        Dispatcher.BeginInvoke(new Action(UpdateProfileCardPosition));
    }

    private void CloseProfileOverlay()
    {
        ProfileOverlay.Visibility = Visibility.Collapsed;
    }

    private void ProfileOverlay_PreviewMouseLeftButtonDown(
        object sender,
        MouseButtonEventArgs e
    )
    {
        var position = e.GetPosition(ProfileCard);
        if (
            position.X >= 0
            && position.Y >= 0
            && position.X <= ProfileCard.ActualWidth
            && position.Y <= ProfileCard.ActualHeight
        )
        {
            return;
        }

        CloseProfileOverlay();
        e.Handled = true;
    }

    private void ShellWindow_PreviewKeyDown(
        object sender,
        System.Windows.Input.KeyEventArgs e
    )
    {
        if (e.Key != Key.Escape)
        {
            return;
        }

        if (StatusFlyoutOverlay.Visibility == Visibility.Visible)
        {
            CloseStatusFlyout();
            e.Handled = true;
            return;
        }

        if (ProfileOverlay.Visibility == Visibility.Visible)
        {
            CloseProfileOverlay();
            e.Handled = true;
        }
    }

    private void ShellRootGrid_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateProfileCardPosition();
        UpdateStatusFlyoutPosition();
    }

    private void ProfileCard_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateProfileCardPosition();
    }

    private void ProfileFrame_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        const double innerCornerRadius = 7;
        if (e.NewSize.Width <= 0 || e.NewSize.Height <= 0)
        {
            ProfileFrame.Clip = null;
            return;
        }

        ProfileFrame.Clip = new RectangleGeometry(
            new Rect(new System.Windows.Point(), e.NewSize),
            innerCornerRadius,
            innerCornerRadius
        );
    }

    private void UpdateProfileCardPosition()
    {
        const double safeMargin = 5;
        if (
            ProfileOverlay.Visibility != Visibility.Visible
            || ShellRootGrid.ActualWidth <= safeMargin * 2
            || ShellRootGrid.ActualHeight <= safeMargin * 2
        )
        {
            return;
        }

        var anchor = Titlebar.GetProfileButtonBounds(ShellRootGrid);
        var availableWidth = Math.Max(0, ShellRootGrid.ActualWidth - safeMargin * 2);
        ProfileCard.MaxWidth = availableWidth;

        var cardWidth = ProfileCard.ActualWidth > 0
            ? Math.Min(ProfileCard.ActualWidth, availableWidth)
            : Math.Min(ProfileCard.Width, availableWidth);
        var desiredLeft = anchor.Left + (anchor.Width - cardWidth) / 2;
        var maximumLeft = Math.Max(safeMargin, ShellRootGrid.ActualWidth - cardWidth - safeMargin);
        var left = Math.Clamp(desiredLeft, safeMargin, maximumLeft);

        var top = Math.Max(safeMargin, anchor.Bottom + safeMargin);
        ProfileCard.MaxHeight = Math.Max(
            0,
            ShellRootGrid.ActualHeight - top - safeMargin
        );

        Canvas.SetLeft(ProfileCard, left);
        Canvas.SetTop(ProfileCard, top);
    }

    private void BackgroundTaskService_TasksChanged(
        object? sender,
        FlourishBackgroundTasksChangedEventArgs e
    )
    {
        var shouldStartRefreshLoop = false;
        lock (backgroundTaskRefreshGate)
        {
            if (isShellClosed)
            {
                return;
            }

            pendingBackgroundTasks = e.Tasks.ToArray();
            backgroundTaskRefreshPending = true;
            if (!backgroundTaskRefreshLoopActive)
            {
                backgroundTaskRefreshLoopActive = true;
                shouldStartRefreshLoop = true;
            }
        }

        if (!shouldStartRefreshLoop)
        {
            return;
        }

        if (Dispatcher.HasShutdownStarted || Dispatcher.HasShutdownFinished)
        {
            ResetBackgroundTaskRefreshLoop();
            return;
        }

        try
        {
            Dispatcher.BeginInvoke(
                DispatcherPriority.Background,
                new Action(StartBackgroundTaskRefreshTimer)
            );
        }
        catch (InvalidOperationException)
        {
            ResetBackgroundTaskRefreshLoop();
        }
    }

    private void BackgroundTaskRefreshTimer_Tick(object? sender, EventArgs e)
    {
        IReadOnlyList<FlourishBackgroundTaskInfo>? tasks = null;
        lock (backgroundTaskRefreshGate)
        {
            if (backgroundTaskRefreshPending)
            {
                tasks = pendingBackgroundTasks;
                backgroundTaskRefreshPending = false;
            }
            else
            {
                backgroundTaskRefreshLoopActive = false;
                backgroundTaskRefreshTimer.Stop();
            }
        }

        if (tasks is not null)
        {
            // Read the service again on the UI thread so an older cross-thread event can never
            // overwrite a newer state after Dispatcher scheduling reorders notifications.
            RefreshBackgroundTaskStatus(backgroundTaskService.ActiveTasks);
        }
    }

    private void StartBackgroundTaskRefreshTimer()
    {
        lock (backgroundTaskRefreshGate)
        {
            if (
                isShellClosed
                || !backgroundTaskRefreshLoopActive
                || Dispatcher.HasShutdownStarted
            )
            {
                return;
            }
        }

        backgroundTaskRefreshTimer.Start();
    }

    private void ResetBackgroundTaskRefreshLoop()
    {
        lock (backgroundTaskRefreshGate)
        {
            backgroundTaskRefreshPending = false;
            backgroundTaskRefreshLoopActive = false;
            pendingBackgroundTasks = [];
        }
    }

    private void UpdateStatusBarVisibility()
    {
        var hasBackgroundTasks = backgroundTasks.Count > 0;
        var showConfiguredContent = options.IsStatusBarEnabled;
        var showStatusBar = showConfiguredContent || hasBackgroundTasks;

        StatusBarBorder.Visibility = showStatusBar
            ? Visibility.Visible
            : Visibility.Collapsed;
        StatusItemsHost.Visibility =
            showConfiguredContent && StatusItemsHost.Children.Count > 0
                ? Visibility.Visible
                : Visibility.Collapsed;
        SystemStatusButton.Visibility =
            showConfiguredContent
            && (
                statusService.IsLANConnectionStatusEnabled
                || statusService.IsPowerStatusEnabled
            )
                ? Visibility.Visible
                : Visibility.Collapsed;
        FooterStartRegionHost.Visibility =
            showConfiguredContent && FooterStartRegionHost.Children.Count > 0
                ? Visibility.Visible
                : Visibility.Collapsed;
        FooterEndRegionHost.Visibility =
            showConfiguredContent && FooterEndRegionHost.Children.Count > 0
                ? Visibility.Visible
                : Visibility.Collapsed;

        if (!showStatusBar && StatusFlyoutOverlay.Visibility == Visibility.Visible)
        {
            CloseStatusFlyout(restoreFocus: false);
        }
    }

    private static void SynchronizePanelChildren(
        WpfPanel panel,
        IReadOnlyList<UIElement> desiredChildren
    )
    {
        for (var index = 0; index < desiredChildren.Count; index++)
        {
            var desired = desiredChildren[index];
            if (
                index < panel.Children.Count
                && ReferenceEquals(panel.Children[index], desired)
            )
            {
                continue;
            }

            var existingIndex = panel.Children.IndexOf(desired);
            if (existingIndex >= 0)
            {
                panel.Children.RemoveAt(existingIndex);
            }

            panel.Children.Insert(index, desired);
        }

        while (panel.Children.Count > desiredChildren.Count)
        {
            panel.Children.RemoveAt(panel.Children.Count - 1);
        }
    }

    private void RefreshBackgroundTaskStatus(
        IReadOnlyList<FlourishBackgroundTaskInfo> tasks
    )
    {
        backgroundTasks = tasks.ToArray();
        var runningTasks = backgroundTasks
            .Where(task =>
                task.State
                is FlourishBackgroundTaskState.Running
                    or FlourishBackgroundTaskState.Cancelling
            )
            .ToArray();
        var queuedTasks = backgroundTasks
            .Where(task => task.State == FlourishBackgroundTaskState.Queued)
            .ToArray();

        var runningTaskIds = runningTasks.Select(task => task.Id).ToHashSet();
        var desiredTaskButtons = new List<UIElement>(runningTasks.Length);
        foreach (var task in runningTasks)
        {
            if (!backgroundTaskIconsById.TryGetValue(task.Id, out var iconView))
            {
                iconView = CreateBackgroundTaskIconView(task);
                backgroundTaskIconsById.Add(task.Id, iconView);
            }

            UpdateBackgroundTaskIconView(iconView, task);
            desiredTaskButtons.Add(iconView.Button);
        }

        SynchronizePanelChildren(BackgroundTaskItemsHost, desiredTaskButtons);
        foreach (var taskId in backgroundTaskIconsById.Keys.Except(runningTaskIds).ToArray())
        {
            backgroundTaskIconsById.Remove(taskId);
        }

        BackgroundTaskQueueButton.Visibility = queuedTasks.Length > 0
            ? Visibility.Visible
            : Visibility.Collapsed;
        BackgroundTaskQueueCountText.Text = queuedTasks.Length.ToString();
        AutomationProperties.SetName(
            BackgroundTaskQueueButton,
            localizationService.Format(
                FlourishLocaleKeys.BackgroundTaskWaitingCount,
                queuedTasks.Length
            )
        );

        UpdateStatusBarVisibility();

        if (statusFlyoutKind != StatusFlyoutKind.BackgroundTasks)
        {
            return;
        }

        if (backgroundTasks.Count == 0)
        {
            CloseStatusFlyout(restoreFocus: false);
            return;
        }

        BuildBackgroundTaskFlyoutContent();
        if (
            statusFlyoutAnchorTaskId is { } anchorTaskId
            && backgroundTaskIconsById.TryGetValue(anchorTaskId, out var anchorIcon)
        )
        {
            statusFlyoutAnchor = anchorIcon.Button;
        }
        else
        {
            statusFlyoutAnchorTaskId = null;
            statusFlyoutAnchor = queuedTasks.Length > 0
                ? BackgroundTaskQueueButton
                : desiredTaskButtons.OfType<FrameworkElement>().FirstOrDefault();
            if (statusFlyoutOpenedWithFocus)
            {
                statusFlyoutRestoreFocusTarget = statusFlyoutAnchor;
            }
        }

        if (statusFlyoutOpenedWithFocus && !StatusFlyoutCard.IsKeyboardFocusWithin)
        {
            FocusStatusFlyoutContent();
        }

        UpdateStatusFlyoutPosition();
    }

    private BackgroundTaskIconView CreateBackgroundTaskIconView(
        FlourishBackgroundTaskInfo task
    )
    {
        var icon = new TextBlock
        {
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            FontFamily = iconFontFamily,
            FontSize = (double)FindResource("FlourishFontSizeCaption"),
        };
        var toolTipName = new TextBlock { FontWeight = FontWeights.SemiBold };
        var toolTipDescription = new TextBlock
        {
            Margin = new Thickness(0, 3, 0, 0),
            MaxWidth = 270,
            TextWrapping = TextWrapping.Wrap,
        };
        toolTipDescription.SetResourceReference(
            TextBlock.ForegroundProperty,
            "MutedTextBrush"
        );
        var toolTipState = new TextBlock { Margin = new Thickness(0, 4, 0, 0) };
        toolTipState.SetResourceReference(TextBlock.ForegroundProperty, "MutedTextBrush");
        var toolTip = new StackPanel();
        toolTip.Children.Add(toolTipName);
        toolTip.Children.Add(toolTipDescription);
        toolTip.Children.Add(toolTipState);

        var button = new Button
        {
            Content = icon,
            Style = (Style)FindResource("FlourishStatusIconButtonStyle"),
            Tag = task.Id,
            ToolTip = toolTip,
        };
        ToolTipService.SetInitialShowDelay(
            button,
            options.Tips.InitialShowDelayMilliseconds
        );
        button.Click += BackgroundTaskIconButton_Click;
        return new BackgroundTaskIconView(
            button,
            icon,
            toolTipName,
            toolTipDescription,
            toolTipState
        );
    }

    private void UpdateBackgroundTaskIconView(
        BackgroundTaskIconView view,
        FlourishBackgroundTaskInfo task
    )
    {
        view.Icon.Text = task.Metadata.IconGlyph ?? "\uE895";
        view.ToolTipName.Text = task.Metadata.Name;
        view.ToolTipDescription.Text = task.Metadata.Description ?? string.Empty;
        view.ToolTipDescription.Visibility = task.Metadata.Description is null
            ? Visibility.Collapsed
            : Visibility.Visible;
        view.ToolTipState.Text = FormatBackgroundTaskState(task);
        AutomationProperties.SetName(
            view.Button,
            $"{task.Metadata.Name}, {GetBackgroundTaskStateText(task.State)}"
        );
    }

    private void BackgroundTaskIconButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement anchor)
        {
            OpenBackgroundTaskFlyout(anchor, focusFlyout: true);
        }
    }

    private void BackgroundTaskQueueButton_MouseEnter(
        object sender,
        System.Windows.Input.MouseEventArgs e
    )
    {
        if (statusFlyoutOpenedWithFocus)
        {
            return;
        }

        OpenBackgroundTaskFlyout(BackgroundTaskQueueButton, focusFlyout: false);
    }

    private void BackgroundTaskQueueButton_Click(object sender, RoutedEventArgs e)
    {
        OpenBackgroundTaskFlyout(BackgroundTaskQueueButton, focusFlyout: true);
    }

    private void OpenBackgroundTaskFlyout(FrameworkElement anchor, bool focusFlyout)
    {
        statusFlyoutAnchorTaskId = anchor.Tag is Guid taskId ? taskId : null;
        BuildBackgroundTaskFlyoutContent();
        OpenStatusFlyout(anchor, StatusFlyoutKind.BackgroundTasks, focusFlyout);
    }

    private void BuildBackgroundTaskFlyoutContent()
    {
        var title = localizationService.Get(FlourishLocaleKeys.BackgroundTaskTitle);
        StatusFlyoutTitle.Text = title;
        AutomationProperties.SetName(StatusFlyoutTitle, title);
        AutomationProperties.SetName(StatusFlyoutCard, title);

        var activeTaskIds = backgroundTasks.Select(task => task.Id).ToHashSet();
        var desiredRows = new List<UIElement>(Math.Max(1, backgroundTasks.Count));
        if (backgroundTasks.Count == 0)
        {
            backgroundTaskEmptyText ??= new TextBlock();
            backgroundTaskEmptyText.Text = localizationService.Get(
                FlourishLocaleKeys.BackgroundTaskNoActiveTasks
            );
            backgroundTaskEmptyText.SetResourceReference(
                TextBlock.ForegroundProperty,
                "MutedTextBrush"
            );
            desiredRows.Add(backgroundTaskEmptyText);
        }
        else
        {
            foreach (var task in backgroundTasks)
            {
                if (!backgroundTaskRowsById.TryGetValue(task.Id, out var rowView))
                {
                    rowView = CreateBackgroundTaskRowView(task);
                    backgroundTaskRowsById.Add(task.Id, rowView);
                }

                UpdateBackgroundTaskRowView(rowView, task);
                desiredRows.Add(rowView.Container);
            }
        }

        SynchronizePanelChildren(StatusFlyoutContentHost, desiredRows);
        foreach (var taskId in backgroundTaskRowsById.Keys.Except(activeTaskIds).ToArray())
        {
            backgroundTaskRowsById.Remove(taskId);
        }
    }

    private BackgroundTaskRowView CreateBackgroundTaskRowView(
        FlourishBackgroundTaskInfo task
    )
    {
        var row = new Border
        {
            Margin = new Thickness(0, 0, 0, 8),
            Padding = new Thickness(10, 8, 10, 8),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
        };
        row.SetResourceReference(Border.BackgroundProperty, "CardSecondaryBackgroundBrush");
        row.SetResourceReference(Border.BorderBrushProperty, "CardBorderBrush");

        var layout = new Grid();
        layout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(24) });
        layout.ColumnDefinitions.Add(
            new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
        );
        layout.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var icon = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Top,
            FontFamily = iconFontFamily,
            FontSize = (double)FindResource("FlourishFontSizeTitlebarIcon"),
        };
        icon.SetResourceReference(TextBlock.ForegroundProperty, "MutedTextBrush");
        layout.Children.Add(icon);

        var details = new StackPanel();
        Grid.SetColumn(details, 1);
        var name = new TextBlock
        {
            FontSize = (double)FindResource("FlourishFontSizeCaption"),
            FontWeight = FontWeights.SemiBold,
            TextTrimming = TextTrimming.CharacterEllipsis,
        };
        details.Children.Add(name);
        var description = new TextBlock
        {
            Margin = new Thickness(0, 2, 8, 0),
            MaxWidth = 220,
            TextWrapping = TextWrapping.Wrap,
        };
        description.SetResourceReference(TextBlock.ForegroundProperty, "MutedTextBrush");
        details.Children.Add(description);

        var state = new TextBlock
        {
            Margin = new Thickness(0, 3, 8, 0),
            FontSize = (double)FindResource("FlourishFontSizeSmall"),
        };
        state.SetResourceReference(TextBlock.ForegroundProperty, "MutedTextBrush");
        details.Children.Add(state);
        layout.Children.Add(details);

        var cancelButton = new Button
        {
            MinWidth = 58,
            Padding = new Thickness(10, 0, 10, 0),
            Tag = task.Id,
            Content = localizationService.Get(FlourishLocaleKeys.BackgroundTaskCancel),
            Style = (Style)FindResource("FlourishActionButtonStyle"),
        };
        Grid.SetColumn(cancelButton, 2);
        cancelButton.Click += CancelBackgroundTaskButton_Click;
        layout.Children.Add(cancelButton);

        row.Child = layout;
        return new BackgroundTaskRowView(
            row,
            icon,
            name,
            description,
            state,
            cancelButton
        );
    }

    private void UpdateBackgroundTaskRowView(
        BackgroundTaskRowView view,
        FlourishBackgroundTaskInfo task
    )
    {
        view.Icon.Text = task.Metadata.IconGlyph ?? "\uE895";
        view.Name.Text = task.Metadata.Name;
        view.Description.Text = task.Metadata.Description ?? string.Empty;
        view.Description.Visibility = task.Metadata.Description is null
            ? Visibility.Collapsed
            : Visibility.Visible;
        view.State.Text = FormatBackgroundTaskState(task);
        view.CancelButton.Tag = task.Id;
        view.CancelButton.IsEnabled = task.State != FlourishBackgroundTaskState.Cancelling;
        AutomationProperties.SetName(
            view.CancelButton,
            $"{localizationService.Get(FlourishLocaleKeys.BackgroundTaskCancel)} {task.Metadata.Name}"
        );
    }

    private void CancelBackgroundTaskButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: Guid taskId })
        {
            backgroundTaskService.CancelTask(taskId);
        }
    }

    private string FormatBackgroundTaskState(FlourishBackgroundTaskInfo task)
    {
        var text = GetBackgroundTaskStateText(task.State);
        return task.Progress is { } progress ? $"{text} · {progress:P0}" : text;
    }

    private string GetBackgroundTaskStateText(FlourishBackgroundTaskState state)
    {
        return localizationService.Get(
            state switch
            {
                FlourishBackgroundTaskState.Queued => FlourishLocaleKeys.BackgroundTaskQueued,
                FlourishBackgroundTaskState.Cancelling => FlourishLocaleKeys.BackgroundTaskCancelling,
                _ => FlourishLocaleKeys.BackgroundTaskRunning,
            }
        );
    }

    private void SystemStatusButton_MouseEnter(
        object sender,
        System.Windows.Input.MouseEventArgs e
    )
    {
        if (statusFlyoutOpenedWithFocus)
        {
            return;
        }

        OpenSystemStatusFlyout(focusFlyout: false);
    }

    private void SystemStatusButton_Click(object sender, RoutedEventArgs e)
    {
        OpenSystemStatusFlyout(focusFlyout: true);
    }

    private void OpenSystemStatusFlyout(bool focusFlyout)
    {
        var title = localizationService.Get(FlourishLocaleKeys.SystemStatusTitle);
        StatusFlyoutTitle.Text = title;
        AutomationProperties.SetName(StatusFlyoutTitle, title);
        AutomationProperties.SetName(StatusFlyoutCard, title);
        StatusFlyoutContentHost.Children.Clear();

        if (statusService.IsLANConnectionStatusEnabled)
        {
            var networkState = localizationService.Get(
                FlourishLocaleKeys.SystemStatusUnknown
            );
            try
            {
                networkState = localizationService.Get(
                    System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable()
                        ? FlourishLocaleKeys.StatusConnected
                        : FlourishLocaleKeys.StatusDisconnected
                );
            }
            catch (Exception error)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"Flourish network status query failed: {error}"
                );
            }

            StatusFlyoutContentHost.Children.Add(
                CreateStatusDetailRow(
                    "\uE701",
                    localizationService.Get(FlourishLocaleKeys.SystemStatusNetwork),
                    networkState
                )
            );
        }

        if (statusService.IsPowerStatusEnabled)
        {
            var powerSource = localizationService.Get(
                FlourishLocaleKeys.SystemStatusUnknown
            );
            try
            {
                var powerStatus = System.Windows.Forms.SystemInformation.PowerStatus;
                powerSource = powerStatus.PowerLineStatus switch
                {
                    System.Windows.Forms.PowerLineStatus.Online => localizationService.Get(
                        FlourishLocaleKeys.SystemStatusAC
                    ),
                    System.Windows.Forms.PowerLineStatus.Offline => localizationService.Get(
                        FlourishLocaleKeys.SystemStatusBattery
                    ),
                    _ => localizationService.Get(FlourishLocaleKeys.SystemStatusUnknown),
                };
                var batteryStatus = powerStatus.BatteryChargeStatus;
                var hasNoSystemBattery = batteryStatus.HasFlag(
                    System.Windows.Forms.BatteryChargeStatus.NoSystemBattery
                );
                var hasUsableBattery =
                    !hasNoSystemBattery
                    && !batteryStatus.HasFlag(
                        System.Windows.Forms.BatteryChargeStatus.Unknown
                    );
                if (
                    hasNoSystemBattery
                    && powerStatus.PowerLineStatus
                        != System.Windows.Forms.PowerLineStatus.Online
                )
                {
                    powerSource = localizationService.Get(
                        FlourishLocaleKeys.SystemStatusUnknown
                    );
                }

                if (
                    hasUsableBattery
                    && powerStatus.BatteryLifePercent is >= 0 and <= 1
                )
                {
                    powerSource = $"{powerSource} · {powerStatus.BatteryLifePercent:P0}";
                }
            }
            catch (Exception error)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"Flourish power status query failed: {error}"
                );
            }

            StatusFlyoutContentHost.Children.Add(
                CreateStatusDetailRow(
                    "\uE850",
                    localizationService.Get(FlourishLocaleKeys.SystemStatusPower),
                    powerSource
                )
            );
        }

        OpenStatusFlyout(SystemStatusButton, StatusFlyoutKind.System, focusFlyout);
    }

    private FrameworkElement CreateStatusDetailRow(
        string iconGlyph,
        string label,
        string value
    )
    {
        var row = new Grid { Margin = new Thickness(0, 0, 0, 8) };
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(24) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var icon = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            FontFamily = iconFontFamily,
            FontSize = (double)FindResource("FlourishFontSizeTitlebarIcon"),
            Text = iconGlyph,
        };
        icon.SetResourceReference(TextBlock.ForegroundProperty, "MutedTextBrush");
        row.Children.Add(icon);

        var text = new StackPanel();
        Grid.SetColumn(text, 1);
        var labelText = new TextBlock
        {
            FontSize = (double)FindResource("FlourishFontSizeSmall"),
            Text = label,
        };
        labelText.SetResourceReference(TextBlock.ForegroundProperty, "MutedTextBrush");
        text.Children.Add(labelText);
        var valueText = new TextBlock
        {
            Margin = new Thickness(0, 2, 0, 0),
            FontSize = (double)FindResource("FlourishFontSizeCaption"),
            Text = value,
        };
        valueText.SetResourceReference(TextBlock.ForegroundProperty, "PrimaryTextBrush");
        text.Children.Add(valueText);
        row.Children.Add(text);
        return row;
    }

    private void OpenStatusFlyout(
        FrameworkElement anchor,
        StatusFlyoutKind kind,
        bool focusFlyout
    )
    {
        CloseProfileOverlay();
        var wasVisible = StatusFlyoutOverlay.Visibility == Visibility.Visible;
        if (focusFlyout && !statusFlyoutOpenedWithFocus)
        {
            statusFlyoutOpenedWithFocus = true;
            statusFlyoutRestoreFocusTarget = anchor;
        }
        else if (!focusFlyout && !wasVisible)
        {
            statusFlyoutOpenedWithFocus = false;
            statusFlyoutRestoreFocusTarget = null;
        }

        if (kind != StatusFlyoutKind.BackgroundTasks)
        {
            statusFlyoutAnchorTaskId = null;
        }

        statusFlyoutAnchor = anchor;
        statusFlyoutKind = kind;
        StatusFlyoutOverlay.Visibility = Visibility.Visible;
        Dispatcher.BeginInvoke(
            new Action(() =>
            {
                UpdateStatusFlyoutPosition();
                if (focusFlyout)
                {
                    FocusStatusFlyoutContent();
                }
            })
        );
    }

    private void FocusStatusFlyoutContent()
    {
        if (statusFlyoutKind == StatusFlyoutKind.BackgroundTasks)
        {
            foreach (var task in backgroundTasks)
            {
                if (
                    backgroundTaskRowsById.TryGetValue(task.Id, out var row)
                    && row.CancelButton.IsEnabled
                    && row.CancelButton.Focus()
                )
                {
                    return;
                }
            }
        }

        if (!StatusFlyoutTitle.Focus())
        {
            StatusFlyoutCard.Focus();
        }
    }

    private void CloseStatusFlyout(bool restoreFocus = true)
    {
        if (StatusFlyoutOverlay.Visibility != Visibility.Visible)
        {
            return;
        }

        StatusFlyoutOverlay.Visibility = Visibility.Collapsed;
        var previousAnchor = statusFlyoutAnchor;
        var shouldRestoreFocus = restoreFocus && statusFlyoutOpenedWithFocus;
        var restoreTarget = statusFlyoutRestoreFocusTarget ?? previousAnchor;
        statusFlyoutAnchor = null;
        statusFlyoutAnchorTaskId = null;
        statusFlyoutKind = StatusFlyoutKind.None;
        statusFlyoutOpenedWithFocus = false;
        statusFlyoutRestoreFocusTarget = null;
        if (shouldRestoreFocus && restoreTarget is not null)
        {
            Keyboard.Focus(restoreTarget);
        }
    }

    private void StatusFlyoutOverlay_PreviewMouseLeftButtonDown(
        object sender,
        MouseButtonEventArgs e
    )
    {
        var position = e.GetPosition(StatusFlyoutCard);
        if (
            position.X >= 0
            && position.Y >= 0
            && position.X <= StatusFlyoutCard.ActualWidth
            && position.Y <= StatusFlyoutCard.ActualHeight
        )
        {
            return;
        }

        CloseStatusFlyout();
        e.Handled = true;
    }

    private void StatusBarBorder_PreviewMouseLeftButtonDown(
        object sender,
        MouseButtonEventArgs e
    )
    {
        if (
            StatusFlyoutOverlay.Visibility != Visibility.Visible
            || statusFlyoutAnchor?.IsMouseOver == true
        )
        {
            return;
        }

        // The status bar is intentionally above the transparent overlay so its buttons remain
        // clickable after a hover-open. Let the new click take focus naturally.
        CloseStatusFlyout(restoreFocus: false);
    }

    private void StatusFlyoutCard_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateStatusFlyoutPosition();
    }

    private void UpdateStatusFlyoutPosition()
    {
        const double safeMargin = 5;
        if (
            StatusFlyoutOverlay.Visibility != Visibility.Visible
            || statusFlyoutAnchor is null
            || ShellRootGrid.ActualWidth <= safeMargin * 2
            || ShellRootGrid.ActualHeight <= safeMargin * 2
        )
        {
            return;
        }

        var anchorTopLeft = statusFlyoutAnchor.TranslatePoint(
            new System.Windows.Point(),
            ShellRootGrid
        );
        var anchor = new Rect(
            anchorTopLeft,
            new System.Windows.Size(
                statusFlyoutAnchor.ActualWidth,
                statusFlyoutAnchor.ActualHeight
            )
        );
        var availableWidth = Math.Max(0, ShellRootGrid.ActualWidth - safeMargin * 2);
        StatusFlyoutCard.MaxWidth = availableWidth;
        StatusFlyoutCard.MaxHeight = Math.Max(0, anchor.Top - safeMargin * 2);

        var cardWidth = StatusFlyoutCard.ActualWidth > 0
            ? Math.Min(StatusFlyoutCard.ActualWidth, availableWidth)
            : Math.Min(StatusFlyoutCard.Width, availableWidth);
        var cardHeight = Math.Min(
            StatusFlyoutCard.ActualHeight,
            StatusFlyoutCard.MaxHeight
        );
        var desiredLeft = anchor.Left + (anchor.Width - cardWidth) / 2;
        var maximumLeft = Math.Max(
            safeMargin,
            ShellRootGrid.ActualWidth - cardWidth - safeMargin
        );
        var left = Math.Clamp(desiredLeft, safeMargin, maximumLeft);
        var top = Math.Max(safeMargin, anchor.Top - cardHeight - safeMargin);

        Canvas.SetLeft(StatusFlyoutCard, left);
        Canvas.SetTop(StatusFlyoutCard, top);
    }

    private void ApplyNavigationPanelPlacement()
    {
        if (options.NavigationPanelDirection == NavigationPanelDirection.Right)
        {
            Grid.SetColumn(ContentAreaGrid, 0);
            Grid.SetColumn(NavigationPaneBorder, 1);
            Grid.SetColumn(NavigationPaneSplitter, 1);
            NavigationPaneSplitter.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            NavigationPaneSplitter.ResizeBehavior = GridResizeBehavior.PreviousAndCurrent;
            NavigationPaneBorder.BorderThickness = new Thickness(1, 0, 0, 0);
            return;
        }

        Grid.SetColumn(NavigationPaneBorder, 0);
        Grid.SetColumn(ContentAreaGrid, 1);
        Grid.SetColumn(NavigationPaneSplitter, 0);
        NavigationPaneSplitter.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
        NavigationPaneSplitter.ResizeBehavior = GridResizeBehavior.CurrentAndNext;
        NavigationPaneBorder.BorderThickness = new Thickness(0, 0, 1, 0);
    }

    private ColumnDefinition GetNavigationPaneColumn()
    {
        return options.NavigationPanelDirection == NavigationPanelDirection.Right
            ? ContentColumn
            : PaneColumn;
    }

    private ColumnDefinition GetContentColumn()
    {
        return options.NavigationPanelDirection == NavigationPanelDirection.Right
            ? PaneColumn
            : ContentColumn;
    }

    private void PreparePaneColumnsForAnimation()
    {
        GetContentColumn().Width = new GridLength(1, GridUnitType.Star);
    }

    private void SetNavigationPaneWidth(double width)
    {
        GetNavigationPaneColumn().Width = new GridLength(width);
        GetContentColumn().Width = new GridLength(1, GridUnitType.Star);
    }

    private void NormalizeNavigationPaneWidths()
    {
        options.OpenPaneWidth = CoerceOpenPaneWidth(options.OpenPaneWidth);
        options.ClosedPaneWidth = Math.Min(options.ClosedPaneWidth, options.OpenPaneWidth);
    }

    private double CoerceOpenPaneWidth(double width)
    {
        return Math.Min(
            Math.Max(width, options.NavigationPaneMinWidth),
            options.NavigationPaneMaxWidth
        );
    }

    private void ApplyNavigationPaneColumnConstraints(bool isOpen)
    {
        var paneColumn = GetNavigationPaneColumn();
        paneColumn.MinWidth = isOpen ? options.NavigationPaneMinWidth : 0;
        paneColumn.MaxWidth = isOpen
            ? options.NavigationPaneMaxWidth
            : double.PositiveInfinity;
    }

    private void UpdateNavigationPaneSplitterState()
    {
        var isSplitterEnabled = options.IsNavigationPanelEnabled && isPaneOpen;
        NavigationPaneSplitter.IsEnabled = isSplitterEnabled;
        NavigationPaneSplitter.Visibility = isSplitterEnabled
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    private void ApplyNavigationPaneState(bool animate = false)
    {
        var isNavigationVisible = options.IsNavigationPanelEnabled;
        var isOpen = isNavigationVisible && isPaneOpen;
        var paneWidth = !isNavigationVisible
            ? 0
            : isOpen
                ? CoerceOpenPaneWidth(options.OpenPaneWidth)
                : options.ClosedPaneWidth;

        if (isOpen || animate)
        {
            ApplyNavigationPaneChrome(isOpen);
        }

        if (!animate)
        {
            ApplyNavigationPaneColumnConstraints(isOpen);
            SetNavigationPaneWidth(paneWidth);
            ApplyNavigationPaneChrome(isOpen);
            UpdateNavigationPaneSplitterState();
            return;
        }

        ApplyNavigationPaneColumnConstraints(false);
        NavigationPaneSplitter.IsEnabled = false;
        NavigationPaneSplitter.Visibility = Visibility.Collapsed;
        PreparePaneColumnsForAnimation();
        var paneColumn = GetNavigationPaneColumn();
        var fromWidth = paneColumn.ActualWidth > 0 ? paneColumn.ActualWidth : paneColumn.Width.Value;

        motionService.AnimateNavigationPane(
            paneColumn,
            fromWidth,
            paneWidth,
            () =>
            {
                ApplyNavigationPaneColumnConstraints(isOpen);
                SetNavigationPaneWidth(paneWidth);
                ApplyNavigationPaneChrome(isOpen);
                UpdateNavigationPaneSplitterState();
            }
        );
    }

    private void NavigationPaneSplitter_DragCompleted(object sender, DragCompletedEventArgs e)
    {
        if (!options.IsNavigationPanelEnabled || !isPaneOpen)
        {
            return;
        }

        var horizontalChange =
            options.NavigationPanelDirection == NavigationPanelDirection.Right
                ? -e.HorizontalChange
                : e.HorizontalChange;
        var paneWidth = e.Canceled
            ? navigationPaneDragStartWidth
            : CoerceOpenPaneWidth(navigationPaneDragStartWidth + horizontalChange);

        options.OpenPaneWidth = paneWidth;
        ApplyNavigationPaneColumnConstraints(isOpen: true);
        SetNavigationPaneWidth(paneWidth);
        RefreshWorkAreaLayout();
    }

    private void NavigationPaneSplitter_DragStarted(object sender, DragStartedEventArgs e)
    {
        navigationPaneDragStartWidth = CoerceOpenPaneWidth(
            GetNavigationPaneColumn().ActualWidth
        );
    }

    private void RefreshWorkAreaLayout()
    {
        WorkAreaGrid.InvalidateMeasure();
        WorkAreaGrid.InvalidateArrange();
        WorkAreaGrid.UpdateLayout();
    }

    private void ApplyNavigationPaneChrome(bool isOpen)
    {
        NavigationItemsHost.Tag = isOpen;
        FixedNavigationItemsHost.Tag = isOpen;
    }

    private void BuildToolbarItems(Type? pageType = null)
    {
        if (!options.IsDynamicToolbarEnabled)
        {
            ToolbarItemsHost.Children.Clear();
            ToolbarHostBorder.Visibility = Visibility.Collapsed;
            activeToolbarPageType = null;
            isDefaultToolbarActive = false;
            return;
        }

        if (activeToolbarPageType == pageType && isDefaultToolbarActive == (pageType is null))
        {
            return;
        }

        ToolbarItemsHost.Children.Clear();
        var toolbarButtons = GetToolbarButtons(pageType);

        foreach (var button in toolbarButtons)
        {
            ToolbarItemsHost.Children.Add(button);
        }

        ToolbarHostBorder.Visibility =
            toolbarButtons.Count > 0
            || ToolbarStartRegionHost.Children.Count > 0
            || ToolbarEndRegionHost.Children.Count > 0
                ? Visibility.Visible
                : Visibility.Collapsed;

        activeToolbarPageType = pageType;
        isDefaultToolbarActive = pageType is null;
    }

    private void BuildRegionContents()
    {
        foreach (var region in Enum.GetValues<FlourishRegion>())
        {
            var elements = options
                .RegionContents.Where(content => content.Region == region)
                .OrderBy(content => content.Order)
                .Select(CreateRegionElement)
                .ToList();

            SetRegionContent(region, elements);
        }
    }

    private FrameworkElement CreateRegionElement(FlourishRegionContent content)
    {
        var element = content.CreateContent(serviceProvider);
        if (element.Parent is not null)
        {
            throw new InvalidOperationException(
                $"The content factory for region {content.Region} returned an element that already has a parent."
            );
        }

        return element;
    }

    private void SetRegionContent(
        FlourishRegion region,
        IReadOnlyList<FrameworkElement> elements
    )
    {
        switch (region)
        {
            case FlourishRegion.TitlebarStart:
            case FlourishRegion.TitlebarCenter:
            case FlourishRegion.TitlebarEnd:
            case FlourishRegion.TitlebarProfile:
                Titlebar.SetRegionContent(region, elements);
                break;
            case FlourishRegion.NavigationHeader:
                SetPanelContent(NavigationHeaderRegionHost, elements);
                break;
            case FlourishRegion.NavigationFooter:
                SetPanelContent(NavigationFooterRegionHost, elements);
                break;
            case FlourishRegion.ContentHeader:
                SetPanelContent(ContentHeaderRegionHost, elements);
                break;
            case FlourishRegion.ContentFooter:
                SetPanelContent(ContentFooterRegionHost, elements);
                break;
            case FlourishRegion.ContentOverlay:
                SetPanelContent(ContentOverlayRegionHost, elements);
                break;
            case FlourishRegion.ToolbarStart:
                SetPanelContent(ToolbarStartRegionHost, elements);
                break;
            case FlourishRegion.ToolbarEnd:
                SetPanelContent(ToolbarEndRegionHost, elements);
                break;
            case FlourishRegion.FooterStart:
                SetPanelContent(FooterStartRegionHost, elements);
                break;
            case FlourishRegion.FooterEnd:
                SetPanelContent(FooterEndRegionHost, elements);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(region), region, "Unknown shell region.");
        }
    }

    private static void SetPanelContent(
        WpfPanel host,
        IReadOnlyList<FrameworkElement> elements
    )
    {
        host.Children.Clear();
        foreach (var element in elements)
        {
            host.Children.Add(element);
        }

        host.Visibility = elements.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private IReadOnlyList<Button> GetToolbarButtons(Type? pageType)
    {
        if (pageType is null)
        {
            return defaultToolbarButtons ??= CreateToolbarButtons(
                toolbarService.GetToolbarItems(),
                showIconOnly: false
            );
        }

        if (!toolbarButtonsByPageType.TryGetValue(pageType, out var toolbarButtons))
        {
            toolbarButtons = CreateToolbarButtons(
                toolbarService.GetToolbarItems(pageType),
                ShouldShowToolbarIconOnly(pageType)
            );
            toolbarButtonsByPageType[pageType] = toolbarButtons;
        }

        return toolbarButtons;
    }

    private IReadOnlyList<Button> CreateToolbarButtons(
        IReadOnlyList<FlourishToolbarItem> items,
        bool showIconOnly
    )
    {
        var buttons = new List<Button>(items.Count);
        foreach (var item in items)
        {
            var useIconOnly = showIconOnly && !string.IsNullOrWhiteSpace(item.IconGlyph);
            var button = new Button
            {
                Content = useIconOnly
                    ? CreateIconContent(item.IconGlyph)
                    : CreateIconTextContent(item.IconGlyph, item.DisplayName),
                Margin = buttons.Count > 0 ? new Thickness(2, 0, 0, 0) : new Thickness(),
                ToolTip = item.DisplayName,
                Style = toolbarButtonStyle,
                Tag = item.CommandKey,
                Width = useIconOnly ? 30 : double.NaN,
            };
            button.Click += ToolbarButton_Click;
            buttons.Add(button);
        }

        return buttons;
    }

    private bool ShouldShowToolbarIconOnly(Type pageType)
    {
        return options.DynamicToolbarIconModes.GetValueOrDefault(pageType, true);
    }

    private void ToolbarButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: string commandKey })
        {
            commandParser.Parse(commandKey);
        }
    }

    private void BuildNavigationItems()
    {
        navigationItemsByKey.Clear();
        navigationItemsByPage.Clear();
        navigationParentsByKey.Clear();
        navigationChildrenByParentKey.Clear();
        activeChildParentItem = null;
        firstNavigationItem = null;

        foreach (var item in options.NavigationItems.Concat(options.FixedNavigationItems))
        {
            item.Validate();
            if (!item.IsNavigationItem)
            {
                continue;
            }

            if (!navigationItemsByKey.ContainsKey(item.Key))
            {
                navigationItemsByKey[item.Key] = item;
            }

            IndexNavigationTreeItem(item);

            if (item.PageType is not null)
            {
                firstNavigationItem ??= item;
                navigationItemsByPage[item.PageType] = item;
            }
        }

        NavigationItemsHost.ItemContainerStyle = navigationListBoxItemStyle;
        NavigationItemsHost.ItemsSource = options.NavigationItems;
        FixedNavigationItemsHost.ItemContainerStyle = navigationListBoxItemStyle;
        FixedNavigationItemsHost.ItemsSource = options.FixedNavigationItems;
        FixedNavigationItemsBorder.Visibility =
            options.FixedNavigationItems.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private void IndexNavigationTreeItem(FlourishNavigationItem item)
    {
        if (item.ParentId != 0)
        {
            navigationParentsByKey[CreateNavigationTreeKey(item, item.ParentId)] = item;
        }

        if (item.ChildId == 0)
        {
            return;
        }

        var childKey = CreateNavigationTreeKey(item, item.ChildId);
        if (!navigationChildrenByParentKey.TryGetValue(childKey, out var children))
        {
            children = [];
            navigationChildrenByParentKey[childKey] = children;
        }

        children.Add(item);
    }

    private void BuildStatusItems()
    {
        StatusItemsHost.Children.Clear();
        var statusFontSize = (double)FindResource("FlourishFontSizeCaption");

        foreach (var item in statusService.StatusItems)
        {
            var status = new StackPanel
            {
                Margin =
                    StatusItemsHost.Children.Count > 0
                        ? new Thickness(14, 0, 0, 0)
                        : new Thickness(),
                Orientation = Orientation.Horizontal,
            };
            var iconText = new TextBlock
            {
                VerticalAlignment = VerticalAlignment.Center,
                FontFamily = iconFontFamily,
                FontSize = statusFontSize,
                Text = item.IconGlyph,
            };
            iconText.SetResourceReference(TextBlock.ForegroundProperty, "MutedTextBrush");
            status.Children.Add(iconText);

            var labelText = new TextBlock
            {
                Margin = new Thickness(5, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = statusFontSize,
                Text = item.Text,
            };
            labelText.SetResourceReference(TextBlock.ForegroundProperty, "MutedTextBrush");
            status.Children.Add(labelText);

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
                FontSize = (double)FindResource("FlourishFontSizeTitlebarIcon"),
                Text = iconGlyph,
            }
        );
        content.Children.Add(
            new TextBlock
            {
                Margin = new Thickness(5, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center,
                Text = label,
            }
        );
        return content;
    }

    private TextBlock CreateIconContent(string iconGlyph)
    {
        return new TextBlock
        {
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            FontFamily = iconFontFamily,
            FontSize = (double)FindResource("FlourishFontSizeTitlebarIcon"),
            Text = iconGlyph,
            TextAlignment = System.Windows.TextAlignment.Center,
        };
    }

    private void NavigateToInitialPage()
    {
        var initialItem =
            GetNavigationItem(options.InitialNavigationKey)
            ?? (
                options.InitialNavigationPageType is null
                    ? null
                    : navigationItemsByPage.GetValueOrDefault(options.InitialNavigationPageType)
            )
            ?? firstNavigationItem;

        if (initialItem is null)
        {
            return;
        }

        SelectNavigationItem(initialItem);
        ActivateNavigationItem(initialItem, false, toggleChildren: false);
    }

    private FlourishNavigationItem? GetNavigationItem(string? key)
    {
        return key is not null && navigationItemsByKey.TryGetValue(key, out var item) ? item : null;
    }

    private void Titlebar_BackRequested(object? sender, EventArgs e)
    {
        if (navigationService.CanGoBack)
        {
            navigationService.GoBack();
        }
    }

    private void Titlebar_ForwardRequested(object? sender, EventArgs e)
    {
        if (navigationService.CanGoForward)
        {
            navigationService.GoForward();
        }
    }

    private void Titlebar_NavigationToggleRequested(object? sender, EventArgs e)
    {
        if (!options.IsNavigationPanelEnabled)
        {
            return;
        }

        var shouldOpen = !isPaneOpen;
        if (!shouldOpen)
        {
            CollapseAllNavigationChildren();
        }

        isPaneOpen = shouldOpen;
        ApplyNavigationPaneState(animate: true);

        if (shouldOpen)
        {
            RestoreSelectedNavigationItem();
        }
    }

    private void NavigationItemsHost_PreviewMouseLeftButtonDown(
        object sender,
        MouseButtonEventArgs e
    )
    {
        if (suppressNavigationSelection || sender is not ListBox)
        {
            return;
        }

        if (GetNavigationItemFromInputSource(e.OriginalSource) is not { } item)
        {
            return;
        }

        e.Handled = true;
        if (!item.IsNavigationItem)
        {
            return;
        }

        OpenNavigationPaneForCollapsedParent(item);

        if (item.IsCommandItem)
        {
            ActivateCommandNavigationItem(item);
            return;
        }

        SelectNavigationItem(item);
        ActivateNavigationItem(item, true);
    }

    private void NavigationItemsHost_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (suppressNavigationSelection || sender is not ListBox listBox)
        {
            return;
        }

        if (e.Key is not (Key.Enter or Key.Space))
        {
            return;
        }

        var item =
            GetNavigationItemFromInputSource(e.OriginalSource)
            ?? listBox.SelectedItem as FlourishNavigationItem;
        if (item is null || !item.IsNavigationItem)
        {
            return;
        }

        e.Handled = true;
        OpenNavigationPaneForCollapsedParent(item);

        if (item.IsCommandItem)
        {
            ActivateCommandNavigationItem(item);
            return;
        }

        SelectNavigationItem(item);
        ActivateNavigationItem(item, true);
    }

    private void NavigationItemsHost_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e
    )
    {
        if (suppressNavigationSelection || sender is not ListBox listBox)
        {
            return;
        }

        if (listBox.SelectedItem is not FlourishNavigationItem item)
        {
            return;
        }

        OpenNavigationPaneForCollapsedParent(item);

        if (!item.IsPageItem)
        {
            RestoreSelectedNavigationItem();
            return;
        }

        SelectNavigationItem(item);
        ActivateNavigationItem(item, true, toggleChildren: false);
    }

    private void ActivateCommandNavigationItem(FlourishNavigationItem item)
    {
        ActivateNavigationItem(item, true);
        RestoreSelectedNavigationItem();
        Keyboard.ClearFocus();
    }

    private void RootFrame_Navigated(object? sender, FlourishNavigatedEventArgs e)
    {
        UpdateTitlebarBreadcrumbNavigation();
        BuildToolbarItems(e.SourcePageType);
        UpdateBreadcrumb(e.SourcePageType);

        SelectNavigationItem(e.SourcePageType);
        motionService.AnimatePageEntrance(RootFrame);
    }

    private void UpdateBreadcrumb(Type sourcePageType)
    {
        if (
            !IsBreadcrumbFeatureEnabled()
        )
        {
            BreadcrumbHost.Visibility = Visibility.Collapsed;
            return;
        }

        if (
            options.BreadcrumbShowOption == BreadcrumbShowOption.Auto
            && !HasBreadcrumbNavigation()
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

    private void ActivateNavigationItem(
        FlourishNavigationItem item,
        bool addToBackStack,
        bool toggleChildren = true
    )
    {
        if (toggleChildren)
        {
            OpenNavigationPaneForCollapsedParent(item);
        }

        if (toggleChildren && item.HasChildren)
        {
            ToggleChildItems(item);
        }

        if (item.IsCommandItem)
        {
            if (!item.HasChildren)
            {
                commandParser.Parse(item.CommandKey);
            }

            return;
        }

        if (item.PageType is null || navigationService.CurrentNavigationKey == item.Key)
        {
            return;
        }

        navigationService.Navigate(item.Key, addToBackStack: addToBackStack);

        if (!addToBackStack && navigationService.CanGoBack)
        {
            navigationService.ClearBackStack();
            UpdateTitlebarBreadcrumbNavigation();
        }
    }

    private void ToggleChildItems(FlourishNavigationItem parent)
    {
        parent.IsExpanded = !parent.IsExpanded;
        SetChildItemsVisibility(parent, parent.IsExpanded);
    }

    private void OpenNavigationPaneForCollapsedParent(FlourishNavigationItem item)
    {
        if (isPaneOpen || !item.HasChildren)
        {
            return;
        }

        isPaneOpen = true;
        ApplyNavigationPaneState(animate: true);
    }

    private void CollapseAllNavigationChildren()
    {
        foreach (var parent in navigationParentsByKey.Values)
        {
            parent.IsExpanded = false;
            SetChildItemsVisibility(parent, false);
        }
    }

    private void ExpandAncestorsForSelection(FlourishNavigationItem item)
    {
        if (item.ChildId == 0)
        {
            return;
        }

        var parent = FindParentNavigationItem(item);
        if (parent is null)
        {
            return;
        }

        parent.IsExpanded = true;
        SetChildItemsVisibility(parent, true);
    }

    private void SetChildItemsVisibility(FlourishNavigationItem parent, bool isVisible)
    {
        foreach (var child in GetChildNavigationItems(parent))
        {
            child.IsVisible = isVisible;
        }
    }

    private IEnumerable<FlourishNavigationItem> GetChildNavigationItems(
        FlourishNavigationItem parent
    )
    {
        return parent.ParentId != 0
            && navigationChildrenByParentKey.TryGetValue(
                CreateNavigationTreeKey(parent, parent.ParentId),
                out var children
            )
            ? children
            : [];
    }

    private FlourishNavigationItem? FindParentNavigationItem(FlourishNavigationItem child)
    {
        return child.ChildId == 0
            ? null
            : navigationParentsByKey.GetValueOrDefault(
                CreateNavigationTreeKey(child, child.ChildId)
            );
    }

    private static NavigationTreeKey CreateNavigationTreeKey(
        FlourishNavigationItem item,
        int relationshipId
    )
    {
        return new NavigationTreeKey(item.IsFixed, item.GroupId, relationshipId);
    }

    private bool IsBreadcrumbFeatureEnabled()
    {
        return options.IsBreadcrumbEnabled
            && options.BreadcrumbShowOption != BreadcrumbShowOption.Hidden;
    }

    private bool HasBreadcrumbNavigation()
    {
        return navigationService.CanGoBack || navigationService.CanGoForward;
    }

    private void UpdateTitlebarBreadcrumbNavigation()
    {
        if (!options.IsTitlebarEnabled)
        {
            return;
        }

        var isVisible = IsBreadcrumbFeatureEnabled()
            && (
                options.BreadcrumbShowOption == BreadcrumbShowOption.Always
                || HasBreadcrumbNavigation()
            );

        Titlebar.SetBreadcrumbNavigationState(
            isVisible,
            navigationService.CanGoBack,
            navigationService.CanGoForward
        );
    }

    private void Titlebar_DragRequested(object? sender, EventArgs e)
    {
        DragMove();
    }

    private void Titlebar_ToggleWindowStateRequested(object? sender, EventArgs e)
    {
        ToggleWindowState();
    }

    private void Titlebar_MinimizeRequested(object? sender, EventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void Titlebar_MaximizeRequested(object? sender, EventArgs e)
    {
        if (ResizeMode is not (ResizeMode.CanResize or ResizeMode.CanResizeWithGrip))
        {
            return;
        }

        ToggleWindowState();
    }

    private void Titlebar_CloseRequested(object? sender, EventArgs e)
    {
        if (trayIconService.IsEnabled)
        {
            if (!trayIconService.MinimizeToTray())
            {
                Close();
            }

            return;
        }

        if (!ConfirmCloseRequest())
        {
            return;
        }

        Close();
    }

    private void Titlebar_ThemeToggleRequested(object? sender, EventArgs e)
    {
        themeService.ToggleTheme();
    }

    private void Titlebar_SearchTextChanged(object? sender, string searchText)
    {
        options.TitlebarSearchTextChanged?.Invoke(serviceProvider, searchText);
    }

    private void ThemeService_ThemeChanged(object? sender, FlourishThemeChangedEventArgs e)
    {
        ApplyThemeState();
    }

    private void ApplyThemeState()
    {
        materialEffectService.SetDarkMode(this, themeService.IsDark);
        Titlebar.SetThemeToggleState(themeService.CurrentTheme, themeService.EffectiveTheme);
    }

    private bool ConfirmCloseRequest()
    {
        FlourishMessageOption[] closeOptions =
        [
            new(
                "no",
                localizationService.Get(FlourishLocaleKeys.MessageBoxNo)
            )
            {
                IsDefault = true,
                IsCancel = true,
            },
            new(
                "yes",
                localizationService.Get(FlourishLocaleKeys.MessageBoxYes)
            )
            {
                IsPrimary = true,
            },
        ];

        return messageService.Show(
            this,
            localizationService.Get(FlourishLocaleKeys.WindowClosePrompt),
            localizationService.Get(FlourishLocaleKeys.WindowCloseTitle),
            closeOptions,
            MessageBoxImage.Question
        )?.Id == "yes";
    }

    private void ShellWindow_Closing(object? sender, CancelEventArgs e)
    {
        if (!trayIconService.IsEnabled || trayIconService.IsExitRequested)
        {
            return;
        }

        e.Cancel = trayIconService.MinimizeToTray();
    }

    private void ShellWindow_Closed(object? sender, EventArgs e)
    {
        lock (backgroundTaskRefreshGate)
        {
            isShellClosed = true;
        }

        backgroundTaskService.TasksChanged -= BackgroundTaskService_TasksChanged;
        backgroundTaskRefreshTimer.Stop();
        backgroundTaskRefreshTimer.Tick -= BackgroundTaskRefreshTimer_Tick;
        ResetBackgroundTaskRefreshLoop();
        profileService.ProfileChanged -= ProfileService_ProfileChanged;
        themeService.ThemeChanged -= ThemeService_ThemeChanged;
        themeService.Detach(this);
    }

    private void MainWindow_StateChanged(object? sender, EventArgs e)
    {
        if (!options.IsTitlebarEnabled)
        {
            return;
        }

        Titlebar.SetMaximized(WindowState == WindowState.Maximized);
    }

    private void ToggleWindowState()
    {
        if (ResizeMode is not (ResizeMode.CanResize or ResizeMode.CanResizeWithGrip))
        {
            return;
        }

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
        if (!item.IsPageItem)
        {
            RestoreSelectedNavigationItem();
            return;
        }

        selectedNavigationItem = item;
        suppressNavigationSelection = true;
        try
        {
            ExpandAncestorsForSelection(item);
            UpdateActiveChildParent(item);

            if (item.IsFixed)
            {
                NavigationItemsHost.SelectedItem = null;
                FixedNavigationItemsHost.SelectedItem = item;
                return;
            }

            FixedNavigationItemsHost.SelectedItem = null;
            NavigationItemsHost.SelectedItem = item;
        }
        finally
        {
            suppressNavigationSelection = false;
        }
    }

    private void RestoreSelectedNavigationItem()
    {
        if (selectedNavigationItem is not null)
        {
            SelectNavigationItem(selectedNavigationItem);
            return;
        }

        suppressNavigationSelection = true;
        try
        {
            NavigationItemsHost.SelectedItem = null;
            FixedNavigationItemsHost.SelectedItem = null;
        }
        finally
        {
            suppressNavigationSelection = false;
        }
    }

    private void UpdateActiveChildParent(FlourishNavigationItem activeItem)
    {
        var parent = activeItem.IsPageItem && activeItem.ChildId != 0
            ? FindParentNavigationItem(activeItem)
            : null;

        if (activeChildParentItem == parent)
        {
            return;
        }

        if (activeChildParentItem is not null)
        {
            activeChildParentItem.IsActiveChildParent = false;
        }

        activeChildParentItem = parent;
        if (activeChildParentItem is not null)
        {
            activeChildParentItem.IsActiveChildParent = true;
        }
    }

    private static T? FindAncestor<T>(DependencyObject source)
        where T : DependencyObject
    {
        var current = source;
        while (current is not null)
        {
            if (current is T match)
            {
                return match;
            }

            current = VisualTreeHelper.GetParent(current);
        }

        return null;
    }

    private static FlourishNavigationItem? GetNavigationItemFromInputSource(object source)
    {
        return source is DependencyObject dependencyObject
            && FindAncestor<ListBoxItem>(dependencyObject)?.DataContext is FlourishNavigationItem item
            ? item
            : null;
    }
}
