namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Describes the active window and page used to resolve scoped keyboard shortcuts.
/// </summary>
public sealed class ShortcutResolutionContext
{
    /// <summary>
    /// Initializes a shortcut resolution context.
    /// </summary>
    /// <param name="windowKey">The optional active window key.</param>
    /// <param name="pageKey">The optional active page key.</param>
    public ShortcutResolutionContext(string? windowKey = null, string? pageKey = null)
    {
        WindowKey = NormalizeOptionalKey(windowKey);
        PageKey = NormalizeOptionalKey(pageKey);
    }

    /// <summary>
    /// Gets the optional active window key.
    /// </summary>
    public string? WindowKey { get; }

    /// <summary>
    /// Gets the optional active page key.
    /// </summary>
    public string? PageKey { get; }

    private static string? NormalizeOptionalKey(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }
}
