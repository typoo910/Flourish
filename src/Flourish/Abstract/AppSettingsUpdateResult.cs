namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Reports the outcome of an appsettings transaction.
/// </summary>
public sealed record AppSettingsUpdateResult(
    string FilePath,
    bool Changed,
    bool ConfigurationReloaded
);
