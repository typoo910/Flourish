namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Describes a runtime font change.
/// </summary>
public sealed class FlourishFontChangedEventArgs(
    string fontFamily,
    string iconFontFamily,
    double fontSize
) : EventArgs
{
    /// <summary>
    /// Gets the current text font family name.
    /// </summary>
    public string FontFamily { get; } = fontFamily;

    /// <summary>
    /// Gets the current icon font family name.
    /// </summary>
    public string IconFontFamily { get; } = iconFontFamily;

    /// <summary>
    /// Gets the current base font size.
    /// </summary>
    public double FontSize { get; } = fontSize;
}
