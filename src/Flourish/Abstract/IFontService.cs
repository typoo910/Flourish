namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Controls Flourish text and icon fonts at runtime.
/// </summary>
public interface IFontService
{
    /// <summary>
    /// Gets the current text font family name.
    /// </summary>
    string FontFamily { get; }

    /// <summary>
    /// Gets the current icon font family name.
    /// </summary>
    string IconFontFamily { get; }

    /// <summary>
    /// Gets the current base font size.
    /// </summary>
    double FontSize { get; }

    /// <summary>
    /// Raised after the runtime font settings change.
    /// </summary>
    /// <remarks>
    /// When a shell window is attached, the event is raised on its dispatcher.
    /// </remarks>
    event EventHandler<FlourishFontChangedEventArgs>? Changed;

    /// <summary>
    /// Changes the text font family and base size together.
    /// </summary>
    void SetFont(string fontFamily, double fontSize);

    /// <summary>
    /// Changes the text font family.
    /// </summary>
    void SetFontFamily(string fontFamily);

    /// <summary>
    /// Changes the base font size.
    /// </summary>
    void SetFontSize(double fontSize);

    /// <summary>
    /// Changes the icon font family.
    /// </summary>
    void SetIconFontFamily(string fontFamily);
}
