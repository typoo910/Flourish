using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TextChangedEventArgs = System.Windows.Controls.TextChangedEventArgs;
using UserControl = System.Windows.Controls.UserControl;

namespace AcksheedSys.Flourish.Control;

internal partial class FlourishTitlebar : UserControl
{
    private const string DefaultIconUri = "pack://application:,,,/Flourish;component/Assets/favicon.ico";
    private static readonly ImageSource? DefaultLogoSource = CreateDefaultLogoSource();

    public FlourishTitlebar()
    {
        InitializeComponent();
    }

    public event EventHandler? BackRequested;

    public event EventHandler? ForwardRequested;

    public event EventHandler? NavigationToggleRequested;

    public event EventHandler? MinimizeRequested;

    public event EventHandler? MaximizeRequested;

    public event EventHandler? CloseRequested;

    public event EventHandler? DragRequested;

    public event EventHandler? ToggleWindowStateRequested;

    public void SetTitle(string title)
    {
        TitleText.Text = title;
    }

    public void SetSubtitle(string subtitle)
    {
        SubtitleText.Text = subtitle;
    }

    public void SetSearchPlaceholder(string placeholder)
    {
        SearchPlaceholderText.Text = placeholder;
        UpdateSearchPlaceholderVisibility();
    }

    public void SetLogo(ImageSource? logoSource, string fallbackText)
    {
        var effectiveLogoSource = logoSource ?? DefaultLogoSource;
        if (effectiveLogoSource is not null)
        {
            LogoImage.Source = effectiveLogoSource;
            LogoImage.Visibility = Visibility.Visible;
            LogoFallback.Visibility = Visibility.Collapsed;
            return;
        }

        LogoFallback.Text = string.IsNullOrWhiteSpace(fallbackText) ? "F" : fallbackText[..1];
        LogoImage.Visibility = Visibility.Collapsed;
        LogoFallback.Visibility = Visibility.Visible;
    }

    public void SetBreadcrumbNavigationState(
        bool isVisible,
        bool canGoBack,
        bool canGoForward
    )
    {
        BreadcrumbNavigationHost.Visibility = ToVisibility(isVisible);
        BackButton.Visibility = ToVisibility(isVisible && (canGoBack || !canGoForward));
        BackButton.IsEnabled = canGoBack;
        ForwardButton.Visibility = ToVisibility(isVisible && canGoForward);
        ForwardButton.IsEnabled = canGoForward;
    }

    public void SetMaximizeEnabled(bool enabled)
    {
        MaximizeButton.IsEnabled = enabled;
    }

    public void SetMaximized(bool isMaximized)
    {
        MaximizeButtonIcon.Text = isMaximized ? "\uE923" : "\uE922";
    }

    public void ConfigureVisibility(
        bool enableSearch,
        bool enableBreadcrumb,
        bool enableNavToggle,
        bool enableLogo,
        bool enableTitle,
        bool enableSubTitle,
        bool enableProfile
    )
    {
        BreadcrumbNavigationHost.Visibility = ToVisibility(enableBreadcrumb);
        NavigationToggleButton.Visibility = ToVisibility(enableNavToggle);
        LogoHost.Visibility = ToVisibility(enableLogo);
        TitleText.Visibility = ToVisibility(enableTitle);
        SubtitleText.Visibility = ToVisibility(enableSubTitle);
        SubtitleText.Margin =
            enableTitle && enableSubTitle ? new Thickness(8, 1, 0, 0) : new Thickness();
        TitleTextHost.Visibility = ToVisibility(enableTitle || enableSubTitle);
        BrandHost.Visibility = ToVisibility(enableLogo || enableTitle || enableSubTitle);
        SearchBoxHost.Visibility = ToVisibility(enableSearch);
        ProfileHost.Visibility = ToVisibility(enableProfile);
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        BackRequested?.Invoke(this, EventArgs.Empty);
    }

    private void ForwardButton_Click(object sender, RoutedEventArgs e)
    {
        ForwardRequested?.Invoke(this, EventArgs.Empty);
    }

    private void NavigationToggleButton_Click(object sender, RoutedEventArgs e)
    {
        NavigationToggleRequested?.Invoke(this, EventArgs.Empty);
    }

    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
        MinimizeRequested?.Invoke(this, EventArgs.Empty);
    }

    private void MaximizeButton_Click(object sender, RoutedEventArgs e)
    {
        MaximizeRequested?.Invoke(this, EventArgs.Empty);
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }

    private void SearchBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        UpdateSearchPlaceholderVisibility();
    }

    private void SearchBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        UpdateSearchPlaceholderVisibility();
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        UpdateSearchPlaceholderVisibility();
    }

    private void Titlebar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState != MouseButtonState.Pressed)
        {
            return;
        }

        if (e.ClickCount == 2)
        {
            ToggleWindowStateRequested?.Invoke(this, EventArgs.Empty);
            return;
        }

        DragRequested?.Invoke(this, EventArgs.Empty);
    }

    private static Visibility ToVisibility(bool visible)
    {
        return visible ? Visibility.Visible : Visibility.Collapsed;
    }

    private static ImageSource? CreateDefaultLogoSource()
    {
        try
        {
            var image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(DefaultIconUri, UriKind.Absolute);
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.EndInit();
            image.Freeze();
            return image;
        }
        catch (InvalidOperationException)
        {
            return null;
        }
        catch (IOException)
        {
            return null;
        }
        catch (NotSupportedException)
        {
            return null;
        }
    }

    private void UpdateSearchPlaceholderVisibility()
    {
        SearchPlaceholderText.Visibility =
            string.IsNullOrEmpty(SearchBox.Text) && !SearchBox.IsKeyboardFocusWithin
                ? Visibility.Visible
                : Visibility.Collapsed;
    }
}
