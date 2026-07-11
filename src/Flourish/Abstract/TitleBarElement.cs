namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Identifies a configurable element in the Flourish title bar.
/// </summary>
public enum TitleBarElement
{
    /// <summary>
    /// The title bar search box.
    /// </summary>
    Search,

    /// <summary>
    /// The breadcrumb trail for the current navigation route.
    /// </summary>
    Breadcrumb,

    /// <summary>
    /// The button that expands or collapses the navigation panel.
    /// </summary>
    NavigationToggle,

    /// <summary>
    /// The application logo or its text fallback.
    /// </summary>
    Logo,

    /// <summary>
    /// The primary application title.
    /// </summary>
    Title,

    /// <summary>
    /// The secondary application subtitle.
    /// </summary>
    Subtitle,

    /// <summary>
    /// The light/dark theme toggle.
    /// </summary>
    ThemeToggle,

    /// <summary>
    /// The profile entry point.
    /// </summary>
    Profile,
}
