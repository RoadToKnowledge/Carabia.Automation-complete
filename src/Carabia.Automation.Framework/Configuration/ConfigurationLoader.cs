using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Carabia.Automation.Framework.Configuration;

/// <summary>
/// Loads, in order (later wins):
///   1. config/appsettings.json
///   2. config/appsettings.{env}.json   (env = CARABIA_AUTOMATION_ENV, default "Dev")
///   3. environment variables  CARABIA_AUTOMATION__Section__Key   (e.g. CARABIA_AUTOMATION__Execution__Headless=true)
///   4. short aliases kept from carabia-ui-tests: CARABIA_BASE_URL, CARABIA_HEADLESS, CARABIA_BROWSER,
///      CARABIA_PRESENTATION_DELAY_MS
/// Then <see cref="SafetyGuard.Apply"/> forces every write switch off for production.
/// </summary>
public static class ConfigurationLoader
{
    private const string EnvironmentVariablePrefix = "CARABIA_AUTOMATION__";
    private static readonly Lazy<TestSettings> Cached = new(LoadFresh);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        Converters = { new JsonStringEnumConverter() }
    };

    /// <summary>Settings are immutable for one test run, so they are loaded once per process.</summary>
    public static TestSettings Load() => Cached.Value;

    public static TestSettings LoadFresh()
    {
        var root = LocateRepositoryRoot();
        var environmentName = Environment.GetEnvironmentVariable("CARABIA_AUTOMATION_ENV")
            ?? Environment.GetEnvironmentVariable("CARABIA_ENV")
            ?? "Dev";

        var configDirectory = Path.Combine(root, "config");
        var basePath = Path.Combine(configDirectory, "appsettings.json");
        if (!File.Exists(basePath))
            throw new FileNotFoundException($"Configuration file not found: {basePath}");

        var merged = ReadJson(basePath);
        var environmentPath = Path.Combine(configDirectory, $"appsettings.{environmentName}.json");
        if (File.Exists(environmentPath))
            Merge(merged, ReadJson(environmentPath));

        ApplyEnvironmentVariables(merged);

        var settings = merged.Deserialize<TestSettings>(JsonOptions) ?? new TestSettings();
        settings.RootDirectory = root;
        settings.EnvironmentName = environmentName;
        SafetyGuard.Apply(settings);
        return settings;
    }

    /// <summary>
    /// Finds the folder containing Carabia.Automation.sln. Falls back to the test output folder,
    /// where the csproj also copies config/ and testdata/.
    /// </summary>
    public static string LocateRepositoryRoot()
    {
        var overrideRoot = Environment.GetEnvironmentVariable("CARABIA_AUTOMATION_ROOT");
        if (!string.IsNullOrWhiteSpace(overrideRoot) && Directory.Exists(overrideRoot))
            return Path.GetFullPath(overrideRoot);

        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Carabia.Automation.sln")))
                return directory.FullName;
            directory = directory.Parent;
        }

        return AppContext.BaseDirectory;
    }

    private static JsonObject ReadJson(string path)
    {
        var node = JsonNode.Parse(File.ReadAllText(path), documentOptions: new JsonDocumentOptions
        {
            CommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        });
        return node as JsonObject ?? throw new InvalidDataException($"{path} must contain a JSON object.");
    }

    private static void Merge(JsonObject target, JsonObject source)
    {
        foreach (var (key, value) in source)
        {
            var existingKey = target.Select(p => p.Key).FirstOrDefault(k => k.Equals(key, StringComparison.OrdinalIgnoreCase)) ?? key;
            if (value is JsonObject sourceObject && target[existingKey] is JsonObject targetObject)
                Merge(targetObject, sourceObject);
            else
                target[existingKey] = value?.DeepClone();
        }
    }

    private static void ApplyEnvironmentVariables(JsonObject merged)
    {
        foreach (System.Collections.DictionaryEntry entry in Environment.GetEnvironmentVariables())
        {
            var name = entry.Key?.ToString() ?? string.Empty;
            if (!name.StartsWith(EnvironmentVariablePrefix, StringComparison.OrdinalIgnoreCase))
                continue;
            var path = name[EnvironmentVariablePrefix.Length..].Split("__", StringSplitOptions.RemoveEmptyEntries);
            if (path.Length > 0)
                SetValue(merged, path, entry.Value?.ToString() ?? string.Empty);
        }

        SetAlias(merged, "CARABIA_BASE_URL", "Execution", "BaseUrl");
        SetAlias(merged, "CARABIA_HEADLESS", "Execution", "Headless");
        SetAlias(merged, "CARABIA_BROWSER", "Execution", "Browser");
        SetAlias(merged, "CARABIA_PRESENTATION_DELAY_MS", "Execution", "PresentationDelayMs");
    }

    private static void SetAlias(JsonObject merged, string variable, params string[] path)
    {
        var value = Environment.GetEnvironmentVariable(variable);
        if (!string.IsNullOrWhiteSpace(value))
            SetValue(merged, path, value);
    }

    /// <summary>Sets Section__Key (objects) or Section__List__0 (array index) like Microsoft.Extensions.Configuration does.</summary>
    private static void SetValue(JsonObject root, IReadOnlyList<string> path, string raw)
    {
        JsonNode current = root;
        for (var i = 0; i < path.Count; i++)
        {
            var isLeaf = i == path.Count - 1;
            if (current is JsonArray array && int.TryParse(path[i], out var index))
            {
                while (array.Count <= index) array.Add(null);
                if (isLeaf) { array[index] = ToJsonValue(raw); return; }
                array[index] ??= int.TryParse(path[i + 1], out _) ? new JsonArray() : new JsonObject();
                current = array[index]!;
                continue;
            }

            var obj = current as JsonObject ?? throw new InvalidDataException($"Cannot apply override for '{string.Join("__", path)}'.");
            var key = obj.Select(p => p.Key).FirstOrDefault(k => k.Equals(path[i], StringComparison.OrdinalIgnoreCase)) ?? path[i];
            if (isLeaf) { obj[key] = ToJsonValue(raw); return; }
            if (obj[key] is not (JsonObject or JsonArray))
                obj[key] = int.TryParse(path[i + 1], out _) ? new JsonArray() : new JsonObject();
            current = obj[key]!;
        }
    }

    private static JsonNode? ToJsonValue(string raw)
    {
        if (bool.TryParse(raw, out var boolean)) return JsonValue.Create(boolean);
        if (long.TryParse(raw, out var number)) return JsonValue.Create(number);
        return JsonValue.Create(raw);
    }
}
