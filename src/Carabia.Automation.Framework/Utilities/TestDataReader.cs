using System.Text.Json;
using Carabia.Automation.Framework.Configuration;

namespace Carabia.Automation.Framework.Utilities;

/// <summary>Reads typed test data from the testdata/ folder. Test logic never hard-codes data values.</summary>
public static class TestDataReader
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    public static T Read<T>(string relativePath) => Read<T>(ConfigurationLoader.Load(), relativePath);

    public static T Read<T>(TestSettings settings, string relativePath)
    {
        var path = Path.Combine(settings.ResolvePath(settings.Execution.TestDataDirectory), relativePath);
        if (!File.Exists(path))
            throw new FileNotFoundException($"Test data file not found: {path}");
        return JsonSerializer.Deserialize<T>(File.ReadAllText(path), Options)
               ?? throw new InvalidDataException($"Test data file {path} is empty.");
    }

    public static string FilePath(TestSettings settings, params string[] parts) =>
        Path.Combine([settings.ResolvePath(settings.Execution.TestDataDirectory), .. parts]);
}
