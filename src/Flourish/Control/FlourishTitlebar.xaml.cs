using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace AcksheedSys.Flourish.Control;

internal partial class FlourishTitlebar : UserControl
{
    public FlourishTitlebar()
    {
        InitializeComponent();
    }

    public event EventHandler? BackRequested;

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
        if (logoSource is not null)
        {
            LogoImage.Source = logoSource;
            LogoImage.Visibility = Visibility.Visible;
            LogoFallback.Visibility = Visibility.Collapsed;
            return;
        }

        LogoFallback.Text = string.IsNullOrWhiteSpace(fallbackText) ? "F" : fallbackText[..1];
        LogoImage.Visibility = Visibility.Collapsed;
        LogoFallback.Visibility = Visibility.Visible;
    }

    public void SetBackEnabled(bool enabled)
    {
        BackButton.IsEnabled = enabled;
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
        bool enableHistoryArrow,
        bool enableNavToggle,
        bool enableLogo,
        bool enableTitle,
        bool enableSubTitle,
        bool enableProfile
    )
    {
        BackButton.Visibility = ToVisibility(enableHistoryArrow);
        NavigationToggleButton.Visibility = ToVisibility(enableNavToggle);
        LogoHost.Visibility = ToVisibility(enableLogo);
        TitleText.Visibility = ToVisibility(enableTitle);
        SubtitleText.Visibility = ToVisibility(enableSubTitle);
        TitleTextHost.Visibility = ToVisibility(enableTitle || enableSubTitle);
        BrandHost.Visibility = ToVisibility(enableLogo || enableTitle || enableSubTitle);
        SearchBoxHost.Visibility = ToVisibility(enableSearch);
        ProfileHost.Visibility = ToVisibility(enableProfile);
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        BackRequested?.Invoke(this, EventArgs.Empty);
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

    private void UpdateSearchPlaceholderVisibility()
    {
        SearchPlaceholderText.Visibility =
            string.IsNullOrEmpty(SearchBox.Text) && !SearchBox.IsKeyboardFocusWithin
                ? Visibility.Visible
                : Visibility.Collapsed;
    }
}
