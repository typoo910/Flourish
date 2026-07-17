using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Controls;

namespace ArkheideSystem.Gallery.Views;

public partial class AppearancePage : Page
{
    private readonly IThemeService theme;
    private readonly IFontService font;
    private readonly IToolTipService toolTips;
    private readonly IMotionService motion;
    private readonly IMaterialEffectService material;
    private bool isRefreshing;

    public AppearancePage(
        IThemeService theme,
        IFontService font,
        IToolTipService toolTips,
        IMotionService motion,
        IMaterialEffectService material
    )
    {
        this.theme = theme;
        this.font = font;
        this.toolTips = toolTips;
        this.motion = motion;
        this.material = material;
        InitializeComponent();

        ThemeBox.ItemsSource = Enum.GetValues<FlourishTheme>();
        PageTransitionBox.ItemsSource = Enum.GetValues<FlourishPageTransition>();
        NavigationTransitionBox.ItemsSource = Enum.GetValues<FlourishNavigationPanelTransition>();
        MaterialBox.ItemsSource = Enum.GetValues<MaterialEffect>();

        Loaded += Page_Loaded;
        Unloaded += Page_Unloaded;
        RefreshAll();
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        Page_Unloaded(sender, e);
        theme.ThemeChanged += RuntimeState_Changed;
        font.Changed += RuntimeState_Changed;
        toolTips.Changed += RuntimeState_Changed;
        motion.Changed += RuntimeState_Changed;
        material.Changed += RuntimeState_Changed;
        RefreshAll();
    }

    private void Page_Unloaded(object sender, RoutedEventArgs e)
    {
        theme.ThemeChanged -= RuntimeState_Changed;
        font.Changed -= RuntimeState_Changed;
        toolTips.Changed -= RuntimeState_Changed;
        motion.Changed -= RuntimeState_Changed;
        material.Changed -= RuntimeState_Changed;
    }

    private void RuntimeState_Changed(object? sender, EventArgs e)
    {
        Dispatcher.BeginInvoke(RefreshAll);
    }

    private void ApplyTheme_Click(object sender, RoutedEventArgs e)
    {
        if (ThemeBox.SelectedItem is FlourishTheme selected)
        {
            Execute(() => theme.SetTheme(selected), ThemeStatusText);
        }
    }

    private void ThemeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CanApplyImmediately)
        {
            ApplyTheme_Click(sender, new RoutedEventArgs());
        }
    }

    private void ToggleTheme_Click(object sender, RoutedEventArgs e)
    {
        Execute(theme.ToggleTheme, ThemeStatusText);
    }

    private void ApplyFont_Click(object sender, RoutedEventArgs e)
    {
        Execute(
            () =>
            {
                font.SetFont(
                    FontFamilyBox.Text,
                    ParseDouble(SmallFontSizeBox.Text, "small font size"),
                    ParseDouble(StandardFontSizeBox.Text, "standard font size"),
                    ParseDouble(IconFontSizeBox.Text, "icon font size"),
                    ParseDouble(LargeFontSizeBox.Text, "large font size"),
                    ParseDouble(ExtraLargeFontSizeBox.Text, "extra-large font size"),
                    ParseDouble(HeaderSizeFontSizeBox.Text, "header font size")
                );
                font.SetIconFontFamily(IconFontFamilyBox.Text);
            },
            FontStatusText
        );
    }

    private void TypographyBox_LostFocus(object sender, RoutedEventArgs e) => CommitTypography();

    private void TypographyBox_KeyDown(object sender, KeyEventArgs e) =>
        CommitOnEnter(e, CommitTypography);

    private void CommitTypography()
    {
        if (CanApplyImmediately)
        {
            ApplyFont_Click(this, new RoutedEventArgs());
        }
    }

    private void ApplyPageFontOverride_Click(object sender, RoutedEventArgs e)
    {
        Execute(
            () =>
            {
                font.SetOverrideFont<AppearancePage>(
                    PageOverrideFontFamilyBox.Text,
                    ParseNullableDouble(
                        PageOverrideSmallFontSizeBox.Text,
                        "page override small font size"
                    ),
                    ParseNullableDouble(
                        PageOverrideStandardFontSizeBox.Text,
                        "page override standard font size"
                    ),
                    ParseNullableDouble(
                        PageOverrideIconFontSizeBox.Text,
                        "page override icon font size"
                    ),
                    ParseNullableDouble(
                        PageOverrideLargeFontSizeBox.Text,
                        "page override large font size"
                    ),
                    ParseNullableDouble(
                        PageOverrideExtraLargeFontSizeBox.Text,
                        "page override extra-large font size"
                    ),
                    ParseNullableDouble(
                        PageOverrideHeaderSizeFontSizeBox.Text,
                        "page override header font size"
                    )
                );
            },
            PageFontOverrideStatusText
        );
    }

    private void PageOverrideBox_LostFocus(object sender, RoutedEventArgs e) =>
        CommitPageOverride();

    private void PageOverrideBox_KeyDown(object sender, KeyEventArgs e) =>
        CommitOnEnter(e, CommitPageOverride);

    private void CommitPageOverride()
    {
        if (CanApplyImmediately)
        {
            ApplyPageFontOverride_Click(this, new RoutedEventArgs());
        }
    }

    private void ClearPageFontOverride_Click(object sender, RoutedEventArgs e)
    {
        Execute(
            () => font.ClearOverrideFont<AppearancePage>(),
            PageFontOverrideStatusText
        );
    }

    private void ConfigureToolTip_Click(object sender, RoutedEventArgs e)
    {
        Execute(
            () =>
                toolTips.Configure(
                    ParseInt(ToolTipDelayBox.Text, "tooltip delay"),
                    ParseDouble(ToolTipMarginBox.Text, "tooltip margin")
                ),
            ToolTipStatusText
        );
    }

    private void ToolTipBox_LostFocus(object sender, RoutedEventArgs e) => CommitToolTips();

    private void ToolTipBox_KeyDown(object sender, KeyEventArgs e) =>
        CommitOnEnter(e, CommitToolTips);

    private void CommitToolTips()
    {
        if (CanApplyImmediately)
        {
            ConfigureToolTip_Click(this, new RoutedEventArgs());
        }
    }

    private void ToggleToolTip_Click(object sender, RoutedEventArgs e)
    {
        Execute(() => toolTips.SetEnabled(!toolTips.Current.IsEnabled), ToolTipStatusText);
    }

    private void ApplyMotion_Click(object sender, RoutedEventArgs e)
    {
        Execute(
            () =>
            {
                if (PageTransitionBox.SelectedItem is not FlourishPageTransition pageTransition
                    || NavigationTransitionBox.SelectedItem
                        is not FlourishNavigationPanelTransition navigationTransition)
                {
                    throw new InvalidOperationException("Select both transition modes.");
                }

                motion.SetPageTransition(
                    pageTransition,
                    TimeSpan.FromMilliseconds(ParseDouble(PageDurationBox.Text, "page duration"))
                );
                motion.SetNavigationPanelTransition(
                    navigationTransition,
                    TimeSpan.FromMilliseconds(
                        ParseDouble(NavigationDurationBox.Text, "navigation duration")
                    )
                );
            },
            MotionStatusText
        );
    }

    private void MotionDurationBox_LostFocus(object sender, RoutedEventArgs e) =>
        CommitMotionDurations();

    private void MotionDurationBox_KeyDown(object sender, KeyEventArgs e) =>
        CommitOnEnter(e, CommitMotionDurations);

    private void CommitMotionDurations()
    {
        if (CanApplyImmediately)
        {
            ApplyMotion_Click(this, new RoutedEventArgs());
        }
    }

    private void PageTransitionBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!CanApplyImmediately || PageTransitionBox.SelectedItem is not FlourishPageTransition transition)
        {
            return;
        }

        Execute(
            () => motion.SetPageTransition(transition, motion.Current.PageTransitionDuration),
            MotionStatusText
        );
    }

    private void NavigationTransitionBox_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e
    )
    {
        if (!CanApplyImmediately
            || NavigationTransitionBox.SelectedItem
                is not FlourishNavigationPanelTransition transition)
        {
            return;
        }

        Execute(
            () =>
                motion.SetNavigationPanelTransition(
                    transition,
                    motion.Current.NavigationPanelTransitionDuration
                ),
            MotionStatusText
        );
    }

    private void MotionEnabledBox_Changed(object sender, RoutedEventArgs e)
    {
        if (CanApplyImmediately)
        {
            Execute(
                () => motion.SetEnabled(MotionEnabledBox.IsChecked == true),
                MotionStatusText
            );
        }
    }

    private void HoverRevealBox_Changed(object sender, RoutedEventArgs e)
    {
        if (CanApplyImmediately)
        {
            Execute(
                () => motion.SetHoverReveal(HoverRevealBox.IsChecked == true),
                MotionStatusText
            );
        }
    }

    private void ReducedMotionBox_Changed(object sender, RoutedEventArgs e)
    {
        if (CanApplyImmediately)
        {
            Execute(
                () => motion.SetRespectSystemReducedMotion(ReducedMotionBox.IsChecked == true),
                MotionStatusText
            );
        }
    }

    private void MaterialBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!CanApplyImmediately || MaterialBox.SelectedItem is not MaterialEffect effect)
        {
            return;
        }

        Execute(() => material.SetEffect(effect), MaterialStatusText);
    }

    private void MaterialDarkModeBox_Changed(object sender, RoutedEventArgs e)
    {
        if (CanApplyImmediately)
        {
            Execute(
                () => material.SetDarkMode(MaterialDarkModeBox.IsChecked == true),
                MaterialStatusText
            );
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
            RefreshAll();
        }
        catch (Exception error)
        {
            status.Text = error.Message;
        }
    }

    private void RefreshAll()
    {
        isRefreshing = true;
        try
        {
            ThemeBox.SelectedItem = theme.CurrentTheme;
            ThemeStatusText.Text =
                $"Requested: {theme.CurrentTheme}  |  Effective: {theme.EffectiveTheme}  |  Dark: {theme.IsDark}";

            FontFamilyBox.Text = font.FontFamily;
            SmallFontSizeBox.Text = font.SmallFontSize.ToString(
                "0.##",
                CultureInfo.CurrentCulture
            );
            StandardFontSizeBox.Text = font.StandardFontSize.ToString(
                "0.##",
                CultureInfo.CurrentCulture
            );
            IconFontSizeBox.Text = font.IconFontSize.ToString(
                "0.##",
                CultureInfo.CurrentCulture
            );
            LargeFontSizeBox.Text = font.LargeFontSize.ToString(
                "0.##",
                CultureInfo.CurrentCulture
            );
            ExtraLargeFontSizeBox.Text = font.ExtraLargeFontSize.ToString(
                "0.##",
                CultureInfo.CurrentCulture
            );
            HeaderSizeFontSizeBox.Text = font.HeaderSizeFontSize.ToString(
                "0.##",
                CultureInfo.CurrentCulture
            );
            IconFontFamilyBox.Text = font.IconFontFamily;
            FontStatusText.Text =
                $"Text: {font.FontFamily}, {FormatScale(font.SmallFontSize, font.StandardFontSize, font.IconFontSize, font.LargeFontSize, font.ExtraLargeFontSize, font.HeaderSizeFontSize)}  |  Icons: {font.IconFontFamily}";

            if (font.PageOverrides.TryGetValue(typeof(AppearancePage), out var pageOverride))
            {
                PageOverrideFontFamilyBox.Text = pageOverride.FontFamily;
                PageOverrideSmallFontSizeBox.Text = pageOverride.SmallFontSize?.ToString(
                    "0.##",
                    CultureInfo.CurrentCulture
                ) ?? string.Empty;
                PageOverrideStandardFontSizeBox.Text = pageOverride.StandardFontSize?.ToString(
                    "0.##",
                    CultureInfo.CurrentCulture
                ) ?? string.Empty;
                PageOverrideIconFontSizeBox.Text = pageOverride.IconFontSize?.ToString(
                    "0.##",
                    CultureInfo.CurrentCulture
                ) ?? string.Empty;
                PageOverrideLargeFontSizeBox.Text = pageOverride.LargeFontSize?.ToString(
                    "0.##",
                    CultureInfo.CurrentCulture
                ) ?? string.Empty;
                PageOverrideExtraLargeFontSizeBox.Text =
                    pageOverride.ExtraLargeFontSize?.ToString(
                        "0.##",
                        CultureInfo.CurrentCulture
                    ) ?? string.Empty;
                PageOverrideHeaderSizeFontSizeBox.Text =
                    pageOverride.HeaderSizeFontSize?.ToString(
                        "0.##",
                        CultureInfo.CurrentCulture
                    ) ?? string.Empty;
                PageFontOverrideStatusText.Text =
                    $"AppearancePage override: {pageOverride.FontFamily}, {FormatScale(pageOverride.SmallFontSize ?? font.SmallFontSize, pageOverride.StandardFontSize ?? font.StandardFontSize, pageOverride.IconFontSize ?? font.IconFontSize, pageOverride.LargeFontSize ?? font.LargeFontSize, pageOverride.ExtraLargeFontSize ?? font.ExtraLargeFontSize, pageOverride.HeaderSizeFontSize ?? font.HeaderSizeFontSize)}.";
            }
            else
            {
                PageOverrideSmallFontSizeBox.Text = string.Empty;
                PageOverrideStandardFontSizeBox.Text = string.Empty;
                PageOverrideIconFontSizeBox.Text = string.Empty;
                PageOverrideLargeFontSizeBox.Text = string.Empty;
                PageOverrideExtraLargeFontSizeBox.Text = string.Empty;
                PageOverrideHeaderSizeFontSizeBox.Text = string.Empty;
                PageFontOverrideStatusText.Text =
                    $"No page override. AppearancePage follows {font.FontFamily}, {FormatScale(font.SmallFontSize, font.StandardFontSize, font.IconFontSize, font.LargeFontSize, font.ExtraLargeFontSize, font.HeaderSizeFontSize)}.";
            }

            var currentToolTips = toolTips.Current;
            ToolTipDelayBox.Text = currentToolTips.InitialShowDelayMilliseconds.ToString(
                CultureInfo.CurrentCulture
            );
            ToolTipMarginBox.Text = currentToolTips.SpawnableMargin.ToString(
                "0.##",
                CultureInfo.CurrentCulture
            );
            ToggleToolTipButton.Content = currentToolTips.IsEnabled
                ? "Disable tooltips"
                : "Enable tooltips";
            ToolTipStatusText.Text =
                $"Enabled: {currentToolTips.IsEnabled}  |  Delay: {currentToolTips.InitialShowDelayMilliseconds} ms  |  Margin: {currentToolTips.SpawnableMargin:0.##}";

            var currentMotion = motion.Current;
            MotionEnabledBox.IsChecked = currentMotion.IsEnabled;
            PageTransitionBox.SelectedItem = currentMotion.PageTransition;
            PageDurationBox.Text = currentMotion.PageTransitionDuration.TotalMilliseconds.ToString(
                "0",
                CultureInfo.CurrentCulture
            );
            NavigationTransitionBox.SelectedItem = currentMotion.NavigationPanelTransition;
            NavigationDurationBox.Text =
                currentMotion.NavigationPanelTransitionDuration.TotalMilliseconds.ToString(
                    "0",
                    CultureInfo.CurrentCulture
                );
            HoverRevealBox.IsChecked = currentMotion.IsHoverRevealEnabled;
            ReducedMotionBox.IsChecked = currentMotion.RespectSystemReducedMotion;
            MotionStatusText.Text =
                $"Animation allowed now: {motion.CanAnimate}  |  Hover duration: {currentMotion.HoverRevealAnimationDuration.TotalMilliseconds:0} ms";

            MaterialBox.SelectedItem = material.CurrentEffect;
            MaterialDarkModeBox.IsChecked = material.IsDarkMode;
            MaterialStatusText.Text =
                $"Requested: {material.CurrentEffect}  |  Supported: {material.IsSupported(material.CurrentEffect)}  |  Applied: {material.IsApplied}";
        }
        finally
        {
            isRefreshing = false;
        }
    }

    private static double ParseDouble(string text, string name)
    {
        if (!double.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out var value))
        {
            throw new ArgumentException($"Enter a valid {name}.");
        }

        return value;
    }

    private static double? ParseNullableDouble(string text, string name)
    {
        return string.IsNullOrWhiteSpace(text) ? null : ParseDouble(text, name);
    }

    private static string FormatScale(
        double smallFontSize,
        double standardFontSize,
        double iconFontSize,
        double largeFontSize,
        double extraLargeFontSize,
        double headerSizeFontSize
    )
    {
        return $"small {smallFontSize:0.##}, standard {standardFontSize:0.##}, icon {iconFontSize:0.##}, large {largeFontSize:0.##}, extra-large {extraLargeFontSize:0.##}, header {headerSizeFontSize:0.##} DIP";
    }

    private static int ParseInt(string text, string name)
    {
        if (!int.TryParse(text, NumberStyles.Integer, CultureInfo.CurrentCulture, out var value))
        {
            throw new ArgumentException($"Enter a valid {name}.");
        }

        return value;
    }
}
