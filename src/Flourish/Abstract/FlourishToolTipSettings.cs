namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Represents runtime tooltip settings.
/// </summary>
public sealed record FlourishToolTipSettings(
    bool IsEnabled,
    int InitialShowDelayMilliseconds,
    double SpawnableMargin
);
