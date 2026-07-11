using System.Collections.ObjectModel;

namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Represents an immutable point-in-time view of effective configuration values.
/// </summary>
public sealed class FlourishConfigurationSnapshot
{
    internal FlourishConfigurationSnapshot(
        long version,
        DateTimeOffset capturedAt,
        IEnumerable<KeyValuePair<string, string?>> values
    )
    {
        Version = version;
        CapturedAt = capturedAt;
        Values = new ReadOnlyDictionary<string, string?>(
            new Dictionary<string, string?>(values, StringComparer.OrdinalIgnoreCase)
        );
    }

    /// <summary>
    /// Gets the monotonically increasing snapshot version.
    /// </summary>
    public long Version { get; }

    /// <summary>
    /// Gets the time at which the snapshot was captured.
    /// </summary>
    public DateTimeOffset CapturedAt { get; }

    /// <summary>
    /// Gets the flattened, colon-delimited effective values.
    /// </summary>
    public IReadOnlyDictionary<string, string?> Values { get; }

    /// <summary>
    /// Gets a value from the snapshot, or <see langword="null" /> when it is absent.
    /// </summary>
    public string? this[string key]
    {
        get
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(key);
            return Values.GetValueOrDefault(key);
        }
    }
}
