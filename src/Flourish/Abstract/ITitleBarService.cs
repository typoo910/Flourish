namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Changes Flourish title bar content and visibility while the application is running.
/// </summary>
public interface ITitleBarService
{
    /// <summary>
    /// Gets an immutable snapshot of the current title bar configuration.
    /// </summary>
    FlourishTitleBarState Current { get; }

    /// <summary>
    /// Occurs synchronously after title bar content or visibility changes.
    /// </summary>
    event EventHandler<FlourishTitleBarChangedEventArgs>? Changed;

    /// <summary>
    /// Sets and shows the primary application title.
    /// </summary>
    /// <param name="title">The non-empty title text.</param>
    /// <exception cref="ArgumentException"><paramref name="title"/> is empty or whitespace.</exception>
    void SetTitle(string title);

    /// <summary>
    /// Sets the subtitle, hiding the subtitle element when no text is supplied.
    /// </summary>
    /// <param name="subtitle">The subtitle text, or <see langword="null"/> to clear it.</param>
    void SetSubtitle(string? subtitle);

    /// <summary>
    /// Changes the title and subtitle atomically and raises one change event.
    /// </summary>
    /// <param name="title">The non-empty title text.</param>
    /// <param name="subtitle">The subtitle text, or <see langword="null"/> to clear and hide it.</param>
    /// <exception cref="ArgumentException"><paramref name="title"/> is empty or whitespace.</exception>
    void SetIdentity(string title, string? subtitle = null);

    /// <summary>
    /// Changes the logo source and optional fallback text and makes the logo element visible.
    /// </summary>
    /// <param name="logoPath">The image path, or <see langword="null"/> to use the text fallback.</param>
    /// <param name="fallbackText">The fallback text, or <see langword="null"/> to retain its current value.</param>
    void SetLogo(string? logoPath, string? fallbackText = null);

    /// <summary>
    /// Sets the placeholder displayed by the title bar search box.
    /// </summary>
    /// <param name="placeholder">The non-empty placeholder text.</param>
    /// <exception cref="ArgumentException"><paramref name="placeholder"/> is empty or whitespace.</exception>
    void SetSearchPlaceholder(string placeholder);

    /// <summary>
    /// Shows or hides one title bar element without changing its content.
    /// </summary>
    /// <param name="element">The element to change.</param>
    /// <param name="visible"><see langword="true"/> to show the element; otherwise, <see langword="false"/>.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="element"/> is not defined.</exception>
    void SetElementVisible(TitleBarElement element, bool visible);

    /// <summary>
    /// Sets when breadcrumbs are displayed and synchronizes breadcrumb visibility.
    /// </summary>
    /// <param name="mode">The breadcrumb presentation mode.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="mode"/> is not defined.</exception>
    void SetBreadcrumbMode(BreadcrumbShowOption mode);
}
