using System.Windows;
using MessageBoxOptions = System.Windows.MessageBoxOptions;

namespace AcksheedSys.Flourish.Abstract;

/// <summary>
/// Shows Flourish-styled modal messages.
/// </summary>
/// <example>
/// <code><![CDATA[
/// public sealed class DemoViewModel(IMessageService messages)
/// {
///     public void Save()
///     {
///         messages.Show("Saved.", "Demo", MessageBoxButton.OK, MessageBoxImage.Information);
///     }
/// }
/// ]]></code>
/// </example>
public interface IMessageService
{
    /// <summary>
    /// Shows a modal message owned by the active application window.
    /// </summary>
    /// <param name="messageBoxText">The message text.</param>
    /// <param name="caption">The dialog caption.</param>
    /// <param name="button">The buttons to display.</param>
    /// <param name="icon">The icon to display.</param>
    /// <param name="defaultResult">The default result.</param>
    /// <param name="options">The message options.</param>
    /// <returns>The selected result.</returns>
    MessageBoxResult Show(
        string messageBoxText,
        string caption = "",
        MessageBoxButton button = MessageBoxButton.OK,
        MessageBoxImage icon = MessageBoxImage.None,
        MessageBoxResult defaultResult = MessageBoxResult.None,
        MessageBoxOptions options = MessageBoxOptions.None
    );

    /// <summary>
    /// Shows a modal message owned by a specific window.
    /// </summary>
    /// <param name="owner">The owner window.</param>
    /// <param name="messageBoxText">The message text.</param>
    /// <param name="caption">The dialog caption.</param>
    /// <param name="button">The buttons to display.</param>
    /// <param name="icon">The icon to display.</param>
    /// <param name="defaultResult">The default result.</param>
    /// <param name="options">The message options.</param>
    /// <returns>The selected result.</returns>
    MessageBoxResult Show(
        Window? owner,
        string messageBoxText,
        string caption = "",
        MessageBoxButton button = MessageBoxButton.OK,
        MessageBoxImage icon = MessageBoxImage.None,
        MessageBoxResult defaultResult = MessageBoxResult.None,
        MessageBoxOptions options = MessageBoxOptions.None
    );
}
