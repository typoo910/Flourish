using System.Windows;
using AcksheedSys.Flourish.Abstract;
using AcksheedSys.Flourish.Windows;
using Application = System.Windows.Application;
using MessageBoxOptions = System.Windows.MessageBoxOptions;

namespace AcksheedSys.Flourish.Services;

internal sealed class MessageService : IMessageService
{
    private const string GenericThemeSource = "/Flourish;component/Themes/Generic.xaml";

    public MessageBoxResult Show(
        string messageBoxText,
        string caption = "",
        MessageBoxButton button = MessageBoxButton.OK,
        MessageBoxImage icon = MessageBoxImage.None,
        MessageBoxResult defaultResult = MessageBoxResult.None,
        MessageBoxOptions options = MessageBoxOptions.None
    )
    {
        return Show(GetActiveOwner(), messageBoxText, caption, button, icon, defaultResult, options);
    }

    public MessageBoxResult Show(
        Window? owner,
        string messageBoxText,
        string caption = "",
        MessageBoxButton button = MessageBoxButton.OK,
        MessageBoxImage icon = MessageBoxImage.None,
        MessageBoxResult defaultResult = MessageBoxResult.None,
        MessageBoxOptions options = MessageBoxOptions.None
    )
    {
        EnsureApplicationResources();

        var dialog = new FlourishMessageBoxWindow(
            messageBoxText,
            caption,
            button,
            icon,
            defaultResult,
            options
        );
        if (owner is not null && owner.IsVisible)
        {
            dialog.Owner = owner;
        }
        else
        {
            dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            dialog.ShowInTaskbar = true;
        }

        dialog.ShowDialog();
        return dialog.Result;
    }

    private static Window? GetActiveOwner()
    {
        var application = Application.Current;
        if (application is null)
        {
            return null;
        }

        foreach (Window window in application.Windows)
        {
            if (window.IsActive)
            {
                return window;
            }
        }

        return application.MainWindow?.IsVisible == true ? application.MainWindow : null;
    }

    private static void EnsureApplicationResources()
    {
        var application = Application.Current;
        if (application is null)
        {
            return;
        }

        if (
            application.Resources.MergedDictionaries.Any(dictionary =>
                dictionary.Source?.OriginalString == GenericThemeSource
            )
        )
        {
            return;
        }

        application.Resources.MergedDictionaries.Add(
            new ResourceDictionary { Source = new Uri(GenericThemeSource, UriKind.Relative) }
        );
    }
}
