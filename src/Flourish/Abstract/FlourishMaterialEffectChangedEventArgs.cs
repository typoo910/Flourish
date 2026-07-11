namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Describes a runtime material effect change.
/// </summary>
public sealed class FlourishMaterialEffectChangedEventArgs(
    MaterialEffect effect,
    bool isSupported,
    bool isApplied,
    bool isDarkMode
) : EventArgs
{
    /// <summary>
    /// Gets the requested effect.
    /// </summary>
    public MaterialEffect Effect { get; } = effect;

    /// <summary>
    /// Gets whether the effect is supported on the current operating system.
    /// </summary>
    public bool IsSupported { get; } = isSupported;

    /// <summary>
    /// Gets whether the effect is currently applied.
    /// </summary>
    public bool IsApplied { get; } = isApplied;

    /// <summary>
    /// Gets whether immersive dark mode is requested.
    /// </summary>
    public bool IsDarkMode { get; } = isDarkMode;
}
