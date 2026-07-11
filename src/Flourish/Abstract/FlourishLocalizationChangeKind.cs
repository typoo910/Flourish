namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Describes why localized values changed.
/// </summary>
public enum FlourishLocalizationChangeKind
{
    /// <summary>
    /// The selected locale changed.
    /// </summary>
    LocaleChanged,

    /// <summary>
    /// A locale file was registered.
    /// </summary>
    FileRegistered,

    /// <summary>
    /// A registered locale file was reloaded.
    /// </summary>
    FileReloaded,

    /// <summary>
    /// A locale-file registration was removed.
    /// </summary>
    FileUnregistered,
}
