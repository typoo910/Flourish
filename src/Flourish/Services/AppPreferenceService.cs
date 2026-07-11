using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using ArkheideSystem.Flourish.Abstract;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace ArkheideSystem.Flourish.Services;

internal sealed class AppPreferenceService(
    IConfiguration configuration,
    IHostEnvironment hostEnvironment
) : IAppSettingsStore
{
    private const string AppSettingsFileName = "appsettings.json";
    private const string ThemeConfigurationKey = "Flourish:Preferences:Theme";
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
    };
    private readonly object gate = new();
    private bool isInvokingEditor;

    public string AppSettingsFilePath => FilePath;

    public string FilePath =>
        Path.Combine(hostEnvironment.ContentRootPath, AppSettingsFileName);

    public FlourishTheme? ReadTheme()
    {
        var value = configuration[ThemeConfigurationKey];
        if (
            string.IsNullOrWhiteSpace(value)
            || !Enum.TryParse(value, ignoreCase: true, out FlourishTheme theme)
            || !Enum.IsDefined(theme)
        )
        {
            return null;
        }

        return theme;
    }

    public void SaveTheme(FlourishTheme theme)
    {
        SetAsync(ThemeConfigurationKey, theme.ToString()).GetAwaiter().GetResult();
    }

    public ValueTask<AppSettingsUpdateResult> UpdateAsync(
        Action<IAppSettingsEditor> update,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(update);
        cancellationToken.ThrowIfCancellationRequested();

        lock (gate)
        {
            if (isInvokingEditor)
            {
                throw new InvalidOperationException(
                    "An appsettings transaction cannot start another transaction."
                );
            }

            cancellationToken.ThrowIfCancellationRequested();
            var root = ReadAppSettings();
            var editor = new AppSettingsEditor(root);
            isInvokingEditor = true;
            try
            {
                update(editor);
            }
            finally
            {
                editor.Complete();
                isInvokingEditor = false;
            }

            if (!editor.IsChanged)
            {
                return ValueTask.FromResult(
                    new AppSettingsUpdateResult(FilePath, false, false)
                );
            }

            cancellationToken.ThrowIfCancellationRequested();
            WriteAppSettings(root);
            var reloaded = ReloadConfiguration();
            return ValueTask.FromResult(
                new AppSettingsUpdateResult(FilePath, true, reloaded)
            );
        }
    }

    public ValueTask<AppSettingsUpdateResult> SetAsync<T>(
        string path,
        T value,
        CancellationToken cancellationToken = default
    )
    {
        return UpdateAsync(editor => editor.Set(path, value), cancellationToken);
    }

    public ValueTask<AppSettingsUpdateResult> RemoveAsync(
        string path,
        CancellationToken cancellationToken = default
    )
    {
        return UpdateAsync(editor => editor.Remove(path), cancellationToken);
    }

    public ValueTask<AppSettingsUpdateResult> MergeAsync<T>(
        string path,
        T value,
        CancellationToken cancellationToken = default
    )
    {
        return UpdateAsync(editor => editor.Merge(path, value), cancellationToken);
    }

    public ValueTask<AppSettingsUpdateResult> AppendAsync<T>(
        string path,
        T value,
        CancellationToken cancellationToken = default
    )
    {
        return UpdateAsync(editor => editor.Append(path, value), cancellationToken);
    }

    private JsonObject ReadAppSettings()
    {
        if (!File.Exists(FilePath))
        {
            return [];
        }

        try
        {
            using var stream = new FileStream(
                FilePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite | FileShare.Delete
            );
            var node = JsonNode.Parse(
                stream,
                documentOptions: new JsonDocumentOptions
                {
                    AllowTrailingCommas = true,
                    CommentHandling = JsonCommentHandling.Skip,
                }
            );
            return node as JsonObject
                ?? throw new InvalidDataException(
                    $"{AppSettingsFileName} must contain a JSON object."
                );
        }
        catch (JsonException error)
        {
            throw new InvalidDataException(
                $"{AppSettingsFileName} contains invalid JSON and was not changed.",
                error
            );
        }
    }

    private void WriteAppSettings(JsonObject root)
    {
        var directory = Path.GetDirectoryName(FilePath)
            ?? throw new InvalidOperationException(
                $"{AppSettingsFileName} has no parent directory."
            );
        Directory.CreateDirectory(directory);
        var temporaryPath = Path.Combine(
            directory,
            $".{AppSettingsFileName}.{Guid.NewGuid():N}.tmp"
        );

        try
        {
            File.WriteAllText(temporaryPath, root.ToJsonString(SerializerOptions));
            File.Move(temporaryPath, FilePath, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }
    }

    private bool ReloadConfiguration()
    {
        if (configuration is not IConfigurationRoot configurationRoot)
        {
            return false;
        }

        configurationRoot.Reload();
        return true;
    }

    private sealed class AppSettingsEditor(JsonObject root) : IAppSettingsEditor
    {
        private bool isActive = true;

        public bool IsChanged { get; private set; }

        public void Set<T>(string path, T value)
        {
            EnsureActive();
            var segments = ParsePath(path);
            var parent = GetOrCreateParent(root, segments);
            var propertyName = segments[^1];
            var existingName = FindPropertyName(parent, propertyName);
            var node = Serialize(value);
            if (
                existingName is not null
                && JsonNode.DeepEquals(parent[existingName], node)
            )
            {
                return;
            }

            parent[existingName ?? propertyName] = node;
            IsChanged = true;
        }

        public bool Remove(string path)
        {
            EnsureActive();
            var segments = ParsePath(path);
            if (!TryGetParent(root, segments, out var parent))
            {
                return false;
            }

            var existingName = FindPropertyName(parent, segments[^1]);
            if (existingName is null || !parent.Remove(existingName))
            {
                return false;
            }

            IsChanged = true;
            return true;
        }

        public void Merge<T>(string path, T value)
        {
            EnsureActive();
            var incoming = Serialize(value) as JsonObject
                ?? throw new ArgumentException(
                    "A merged appsettings value must serialize to a JSON object.",
                    nameof(value)
                );
            var segments = ParsePath(path);
            var parent = GetOrCreateParent(root, segments);
            var propertyName = segments[^1];
            var existingName = FindPropertyName(parent, propertyName);
            JsonObject target;
            if (existingName is null)
            {
                target = [];
                parent[propertyName] = target;
                IsChanged = true;
            }
            else
            {
                target = parent[existingName] as JsonObject
                    ?? throw new InvalidDataException(
                        $"The {AppSettingsFileName} value '{path}' must be a JSON object."
                    );
            }

            IsChanged |= MergeObjects(target, incoming);
        }

        public void Append<T>(string path, T value)
        {
            EnsureActive();
            var segments = ParsePath(path);
            var parent = GetOrCreateParent(root, segments);
            var propertyName = segments[^1];
            var existingName = FindPropertyName(parent, propertyName);
            JsonArray target;
            if (existingName is null)
            {
                target = [];
                parent[propertyName] = target;
            }
            else
            {
                target = parent[existingName] as JsonArray
                    ?? throw new InvalidDataException(
                        $"The {AppSettingsFileName} value '{path}' must be a JSON array."
                    );
            }

            target.Add(Serialize(value));
            IsChanged = true;
        }

        public void Complete()
        {
            isActive = false;
        }

        private void EnsureActive()
        {
            ObjectDisposedException.ThrowIf(!isActive, this);
        }

        private static JsonNode? Serialize<T>(T value)
        {
            return JsonSerializer.SerializeToNode(value, SerializerOptions);
        }

        private static string[] ParsePath(string path)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(path);
            var segments = path.Split(':', StringSplitOptions.TrimEntries);
            if (segments.Any(string.IsNullOrWhiteSpace))
            {
                throw new ArgumentException(
                    "An appsettings path cannot contain an empty segment.",
                    nameof(path)
                );
            }

            return segments;
        }

        private static JsonObject GetOrCreateParent(JsonObject root, string[] segments)
        {
            var current = root;
            for (var index = 0; index < segments.Length - 1; index++)
            {
                var segment = segments[index];
                var existingName = FindPropertyName(current, segment);
                if (existingName is null)
                {
                    var created = new JsonObject();
                    current[segment] = created;
                    current = created;
                    continue;
                }

                current = current[existingName] as JsonObject
                    ?? throw new InvalidDataException(
                        $"The {AppSettingsFileName} value '{string.Join(':', segments[..(index + 1)])}' must be a JSON object."
                    );
            }

            return current;
        }

        private static bool TryGetParent(
            JsonObject root,
            string[] segments,
            out JsonObject parent
        )
        {
            parent = root;
            for (var index = 0; index < segments.Length - 1; index++)
            {
                var existingName = FindPropertyName(parent, segments[index]);
                if (existingName is null || parent[existingName] is not JsonObject child)
                {
                    return false;
                }

                parent = child;
            }

            return true;
        }

        private static bool MergeObjects(JsonObject target, JsonObject incoming)
        {
            var changed = false;
            foreach (var (name, incomingValue) in incoming)
            {
                var existingName = FindPropertyName(target, name);
                if (
                    incomingValue is JsonObject incomingObject
                    && existingName is not null
                    && target[existingName] is JsonObject targetObject
                )
                {
                    changed |= MergeObjects(targetObject, incomingObject);
                    continue;
                }

                if (
                    existingName is not null
                    && JsonNode.DeepEquals(target[existingName], incomingValue)
                )
                {
                    continue;
                }

                target[existingName ?? name] = incomingValue?.DeepClone();
                changed = true;
            }

            return changed;
        }

        private static string? FindPropertyName(JsonObject parent, string propertyName)
        {
            return parent
                .Select(property => property.Key)
                .FirstOrDefault(existingName =>
                    string.Equals(
                        existingName,
                        propertyName,
                        StringComparison.OrdinalIgnoreCase
                    )
                );
        }
    }
}
