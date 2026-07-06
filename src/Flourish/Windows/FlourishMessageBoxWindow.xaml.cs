using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Brush = System.Windows.Media.Brush;
using Button = System.Windows.Controls.Button;
using Color = System.Windows.Media.Color;
using Key = System.Windows.Input.Key;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBoxOptions = System.Windows.MessageBoxOptions;
using WpfFlowDirection = System.Windows.FlowDirection;

namespace AcksheedSys.Flourish.Windows;

internal partial class FlourishMessageBoxWindow : Window
{
    private readonly MessageBoxButton buttons;
    private MessageBoxResult result = MessageBoxResult.None;

    public FlourishMessageBoxWindow(
        string messageBoxText,
        string caption,
        MessageBoxButton buttons,
        MessageBoxImage icon,
        MessageBoxResult defaultResult,
        MessageBoxOptions options
    )
    {
        InitializeComponent();

        this.buttons = buttons;
        Title = caption;
        CaptionText.Text = caption;
        CaptionText.Visibility = string.IsNullOrWhiteSpace(caption)
            ? Visibility.Collapsed
            : Visibility.Visible;
        MessageText.Text = messageBoxText;
        MessageText.TextAlignment = options.HasFlag(MessageBoxOptions.RightAlign)
            ? TextAlignment.Right
            : TextAlignment.Left;
        FlowDirection = options.HasFlag(MessageBoxOptions.RtlReading)
            ? WpfFlowDirection.RightToLeft
            : WpfFlowDirection.LeftToRight;

        ConfigureIcon(icon);
        ConfigureButtons(defaultResult);
        CloseButton.IsEnabled = CanClose(buttons);
    }

    public MessageBoxResult Result => result;

    protected override void OnClosing(CancelEventArgs e)
    {
        if (result == MessageBoxResult.None)
        {
            if (!CanClose(buttons))
            {
                e.Cancel = true;
                return;
            }

            result = GetCloseResult(buttons);
        }

        base.OnClosing(e);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape && CanClose(buttons))
        {
            result = GetCloseResult(buttons);
            DialogResult = false;
            e.Handled = true;
            return;
        }

        base.OnKeyDown(e);
    }

    private void ConfigureIcon(MessageBoxImage icon)
    {
        var iconGlyph = GetIconGlyph(icon);
        if (iconGlyph is null)
        {
            IconHost.Visibility = Visibility.Collapsed;
            return;
        }

        IconText.Text = iconGlyph;
        var (background, foreground) = GetIconColors(icon);
        IconHost.Background = background;
        IconText.Foreground = foreground;
    }

    private void ConfigureButtons(MessageBoxResult defaultResult)
    {
        var buttonResults = GetButtonResults(buttons);
        var effectiveDefaultResult = GetEffectiveDefaultResult(buttons, defaultResult);

        foreach (var buttonResult in buttonResults)
        {
            var button = new Button
            {
                Content = GetButtonText(buttonResult),
                IsDefault = buttonResult == effectiveDefaultResult,
                IsCancel = IsCancelResult(buttons, buttonResult),
                Margin =
                    ButtonsHost.Children.Count > 0
                        ? new Thickness(8, 0, 0, 0)
                        : new Thickness(),
                Style = (Style)FindResource(
                    buttonResult == effectiveDefaultResult
                        ? "FlourishMessageBoxPrimaryButtonStyle"
                        : "FlourishMessageBoxButtonStyle"
                ),
                Tag = buttonResult,
            };
            button.Click += ResultButton_Click;
            ButtonsHost.Children.Add(button);
        }
    }

    private void ResultButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: MessageBoxResult selectedResult })
        {
            result = selectedResult;
            DialogResult = true;
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        result = GetCloseResult(buttons);
        DialogResult = false;
    }

    private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed)
        {
            DragMove();
        }
    }

    private static IReadOnlyList<MessageBoxResult> GetButtonResults(MessageBoxButton buttons)
    {
        return buttons switch
        {
            MessageBoxButton.OKCancel => [MessageBoxResult.OK, MessageBoxResult.Cancel],
            MessageBoxButton.YesNo => [MessageBoxResult.Yes, MessageBoxResult.No],
            MessageBoxButton.YesNoCancel => [
                MessageBoxResult.Yes,
                MessageBoxResult.No,
                MessageBoxResult.Cancel,
            ],
            _ => [MessageBoxResult.OK],
        };
    }

    private static MessageBoxResult GetEffectiveDefaultResult(
        MessageBoxButton buttons,
        MessageBoxResult defaultResult
    )
    {
        var availableResults = GetButtonResults(buttons);
        if (defaultResult != MessageBoxResult.None && availableResults.Contains(defaultResult))
        {
            return defaultResult;
        }

        return availableResults[0];
    }

    private static bool IsCancelResult(MessageBoxButton buttons, MessageBoxResult result)
    {
        return buttons switch
        {
            MessageBoxButton.OK => result == MessageBoxResult.OK,
            MessageBoxButton.OKCancel or MessageBoxButton.YesNoCancel =>
                result == MessageBoxResult.Cancel,
            _ => false,
        };
    }

    private static bool CanClose(MessageBoxButton buttons)
    {
        return buttons != MessageBoxButton.YesNo;
    }

    private static MessageBoxResult GetCloseResult(MessageBoxButton buttons)
    {
        return buttons switch
        {
            MessageBoxButton.OK => MessageBoxResult.OK,
            MessageBoxButton.OKCancel or MessageBoxButton.YesNoCancel => MessageBoxResult.Cancel,
            MessageBoxButton.YesNo => MessageBoxResult.No,
            _ => MessageBoxResult.None,
        };
    }

    private static string GetButtonText(MessageBoxResult result)
    {
        return result switch
        {
            MessageBoxResult.OK => "OK",
            MessageBoxResult.Cancel => "Cancel",
            MessageBoxResult.Yes => "Yes",
            MessageBoxResult.No => "No",
            _ => result.ToString(),
        };
    }

    private static string? GetIconGlyph(MessageBoxImage icon)
    {
        if (icon == MessageBoxImage.None)
        {
            return null;
        }

        if (icon == MessageBoxImage.Hand)
        {
            return "\uEA39";
        }

        if (icon == MessageBoxImage.Question)
        {
            return "\uE897";
        }

        if (icon == MessageBoxImage.Exclamation)
        {
            return "\uE7BA";
        }

        return "\uE946";
    }

    private static (Brush Background, Brush Foreground) GetIconColors(MessageBoxImage icon)
    {
        if (icon == MessageBoxImage.Hand)
        {
            return (CreateBrush(0xFEE2E2), CreateBrush(0xDC2626));
        }

        if (icon == MessageBoxImage.Question)
        {
            return (CreateBrush(0xEDE9FE), CreateBrush(0x7C3AED));
        }

        if (icon == MessageBoxImage.Exclamation)
        {
            return (CreateBrush(0xFEF3C7), CreateBrush(0xD97706));
        }

        return (CreateBrush(0xDBEAFE), CreateBrush(0x2563EB));
    }

    private static SolidColorBrush CreateBrush(uint rgb)
    {
        return new SolidColorBrush(
            Color.FromRgb((byte)(rgb >> 16), (byte)(rgb >> 8), (byte)rgb)
        );
    }
}
