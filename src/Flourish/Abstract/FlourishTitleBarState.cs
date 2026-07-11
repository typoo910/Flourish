namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Represents an immutable snapshot of the current Flourish title bar state.
/// </summary>
/// <param name="Title">The primary application title.</param>
/// <param name="Subtitle">The secondary application subtitle, or an empty string when unset.</param>
/// <param name="SearchPlaceholder">The search-box placeholder.</param>
/// <param name="LogoPath">The configured logo path, or <see langword="null"/> when the text fallback is used.</param>
/// <param name="LogoFallbackText">The text displayed when no logo image is available.</param>
/// <param name="IsSearchVisible">Whether the search box is visible.</param>
/// <param name="IsBreadcrumbVisible">Whether the breadcrumb trail is visible.</param>
/// <param name="IsNavigationToggleVisible">Whether the navigation toggle is visible.</param>
/// <param name="IsLogoVisible">Whether the application logo is visible.</param>
/// <param name="IsTitleVisible">Whether the primary title is visible.</param>
/// <param name="IsSubtitleVisible">Whether the subtitle is visible.</param>
/// <param name="IsThemeToggleVisible">Whether the theme toggle is visible.</param>
/// <param name="IsProfileVisible">Whether the profile entry point is visible.</param>
/// <param name="BreadcrumbMode">The current breadcrumb presentation mode.</param>
public sealed record FlourishTitleBarState(
    string Title,
    string Subtitle,
    string SearchPlaceholder,
    string? LogoPath,
    string LogoFallbackText,
    bool IsSearchVisible,
    bool IsBreadcrumbVisible,
    bool IsNavigationToggleVisible,
    bool IsLogoVisible,
    bool IsTitleVisible,
    bool IsSubtitleVisible,
    bool IsThemeToggleVisible,
    bool IsProfileVisible,
    BreadcrumbShowOption BreadcrumbMode
);
