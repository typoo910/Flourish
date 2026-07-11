namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Describes a runtime theme change.
/// </summary>
public sealed class FlourishThemeChangedEventArgs(
    FlourishTheme requestedTheme,
    FlourishTheme effectiveTheme
) : EventArgs
{
    /// <summary>
    /// Gets the selected System, Light, or Dark mode.
    /// </summary>
    public FlourishTheme RequestedTheme { get; } = requestedTheme;

    /// <summary>
    /// Gets the resolved Light or Dark mode.
    /// </summary>
    public FlourishTheme EffectiveTheme { get; } = effectiveTheme;

    /// <summary>
    /// Gets a value indicating whether the resolved theme is dark.
    /// </summary>
    public bool IsDark => EffectiveTheme == FlourishTheme.Dark;
}
