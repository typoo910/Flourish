namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Describes a runtime font change.
/// </summary>
public sealed class FlourishFontChangedEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a font change notification with its affected scope.
    /// </summary>
    public FlourishFontChangedEventArgs(
        string fontFamily,
        string iconFontFamily,
        double smallFontSize,
        double standardFontSize,
        double iconFontSize,
        double largeFontSize,
        double extraLargeFontSize,
        double headerSizeFontSize,
        FlourishFontChangeKind changeKind,
        Type? affectedPageType = null
    )
    {
        if (string.IsNullOrWhiteSpace(fontFamily))
        {
            throw new ArgumentException("Value cannot be empty.", nameof(fontFamily));
        }

        if (string.IsNullOrWhiteSpace(iconFontFamily))
        {
            throw new ArgumentException("Value cannot be empty.", nameof(iconFontFamily));
        }

        ValidateFontScale(
            smallFontSize,
            standardFontSize,
            iconFontSize,
            largeFontSize,
            extraLargeFontSize,
            headerSizeFontSize
        );

        if (!Enum.IsDefined(changeKind))
        {
            throw new ArgumentOutOfRangeException(nameof(changeKind), changeKind, "Unknown value.");
        }

        if (changeKind == FlourishFontChangeKind.PageOverride && affectedPageType is null)
        {
            throw new ArgumentNullException(nameof(affectedPageType));
        }

        FontFamily = fontFamily;
        IconFontFamily = iconFontFamily;
        SmallFontSize = smallFontSize;
        StandardFontSize = standardFontSize;
        IconFontSize = iconFontSize;
        LargeFontSize = largeFontSize;
        ExtraLargeFontSize = extraLargeFontSize;
        HeaderSizeFontSize = headerSizeFontSize;
        ChangeKind = changeKind;
        AffectedPageType = affectedPageType;
    }

    /// <summary>
    /// Gets the current text font family name.
    /// </summary>
    public string FontFamily { get; }

    /// <summary>
    /// Gets the current icon font family name.
    /// </summary>
    public string IconFontFamily { get; }

    /// <summary>
    /// Gets the current small font size.
    /// </summary>
    public double SmallFontSize { get; }

    /// <summary>
    /// Gets the current standard font size.
    /// </summary>
    public double StandardFontSize { get; }

    /// <summary>
    /// Gets the current icon font size.
    /// </summary>
    public double IconFontSize { get; }

    /// <summary>
    /// Gets the current large font size.
    /// </summary>
    public double LargeFontSize { get; }

    /// <summary>
    /// Gets the current extra-large font size.
    /// </summary>
    public double ExtraLargeFontSize { get; }

    /// <summary>
    /// Gets the current header-size font size.
    /// </summary>
    public double HeaderSizeFontSize { get; }

    /// <summary>
    /// Gets the scope of the change.
    /// </summary>
    public FlourishFontChangeKind ChangeKind { get; }

    /// <summary>
    /// Gets the configured page type affected by an override change, when applicable.
    /// </summary>
    public Type? AffectedPageType { get; }

    private static void ValidateFontScale(
        double smallFontSize,
        double standardFontSize,
        double iconFontSize,
        double largeFontSize,
        double extraLargeFontSize,
        double headerSizeFontSize
    )
    {
        ValidatePositiveFinite(smallFontSize, nameof(smallFontSize));
        ValidatePositiveFinite(standardFontSize, nameof(standardFontSize));
        ValidatePositiveFinite(iconFontSize, nameof(iconFontSize));
        ValidatePositiveFinite(largeFontSize, nameof(largeFontSize));
        ValidatePositiveFinite(extraLargeFontSize, nameof(extraLargeFontSize));
        ValidatePositiveFinite(headerSizeFontSize, nameof(headerSizeFontSize));
    }

    private static void ValidatePositiveFinite(double value, string parameterName)
    {
        if (!double.IsFinite(value) || value <= 0)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "Value must be a positive finite number."
            );
        }
    }
}
