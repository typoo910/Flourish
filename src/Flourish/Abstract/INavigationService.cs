namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Provides runtime navigation services for registered Flourish pages.
/// </summary>
/// <example>
/// <code><![CDATA[
/// public sealed class HomeViewModel(INavigationService navigation)
/// {
///     public void OpenSettings() => navigation.Navigate("Settings");
/// }
/// ]]></code>
/// </example>
public interface INavigationService
{
    /// <summary>
    /// Occurs after Flourish navigates to a registered page.
    /// </summary>
    /// <example>
    /// <code><![CDATA[
    /// navigation.Navigated += (_, args) =>
    /// {
    ///     Console.WriteLine(args.SourcePageType.Name);
    /// };
    /// ]]></code>
    /// </example>
    event EventHandler<FlourishNavigatedEventArgs>? Navigated;

    /// <summary>
    /// Gets a value indicating whether backward navigation is available.
    /// </summary>
    /// <example>
    /// <code><![CDATA[
    /// if (navigation.CanGoBack)
    /// {
    ///     navigation.GoBack();
    /// }
    /// ]]></code>
    /// </example>
    bool CanGoBack { get; }

    /// <summary>
    /// Gets a value indicating whether forward navigation is available.
    /// </summary>
    /// <example>
    /// <code><![CDATA[
    /// if (navigation.CanGoForward)
    /// {
    ///     navigation.GoForward();
    /// }
    /// ]]></code>
    /// </example>
    bool CanGoForward { get; }

    /// <summary>
    /// Gets the registered source page type currently displayed in the content frame.
    /// </summary>
    /// <example>
    /// <code><![CDATA[
    /// Type? currentPageType = navigation.CurrentSourcePageType;
    /// ]]></code>
    /// </example>
    Type? CurrentSourcePageType { get; }

    /// <summary>
    /// Gets the navigation key currently displayed in the content frame.
    /// </summary>
    /// <example>
    /// <code><![CDATA[
    /// string? currentRoute = navigation.CurrentNavigationKey;
    /// ]]></code>
    /// </example>
    string? CurrentNavigationKey { get; }

    /// <summary>
    /// Navigates to a registered page by navigation key.
    /// </summary>
    /// <param name="navigationKey">The case-sensitive key generated from the registered Page class name.</param>
    /// <param name="parameter">An optional parameter passed to the destination page.</param>
    /// <param name="addToBackStack">A value indicating whether the current page should be added to the back stack.</param>
    /// <returns><see langword="true" /> if navigation succeeded; otherwise, <see langword="false" />.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the service is not initialized or <paramref name="navigationKey" /> is not registered.</exception>
    /// <example>
    /// <code><![CDATA[
    /// navigation.Navigate("Settings");
    /// ]]></code>
    /// </example>
    bool Navigate(string navigationKey, object? parameter = null, bool addToBackStack = true);

    /// <summary>
    /// Navigates to the previous page in the back stack.
    /// </summary>
    /// <returns><see langword="true" /> if backward navigation succeeded; otherwise, <see langword="false" />.</returns>
    /// <example>
    /// <code><![CDATA[
    /// var moved = navigation.GoBack();
    /// ]]></code>
    /// </example>
    bool GoBack();

    /// <summary>
    /// Navigates to the next page in the forward stack.
    /// </summary>
    /// <returns><see langword="true" /> if forward navigation succeeded; otherwise, <see langword="false" />.</returns>
    /// <example>
    /// <code><![CDATA[
    /// var moved = navigation.GoForward();
    /// ]]></code>
    /// </example>
    bool GoForward();

    /// <summary>
    /// Clears the navigation back stack.
    /// </summary>
    /// <example>
    /// <code><![CDATA[
    /// navigation.ClearBackStack();
    /// ]]></code>
    /// </example>
    void ClearBackStack();
}
