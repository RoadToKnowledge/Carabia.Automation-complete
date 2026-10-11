namespace Carabia.Automation.Framework.Utilities;

/// <summary>
/// Every record the automation creates carries a marker "AUTO-yyyyMMddHHmmss-xxxx".
/// Destructive steps only ever touch rows whose visible text contains such a marker
/// created in the same test.
/// </summary>
public static class RandomDataGenerator
{
    public const string MarkerPrefix = "AUTO-";

    public static string AutomationMarker() =>
        $"{MarkerPrefix}{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..4].ToUpperInvariant()}";

    public static bool IsAutomationMarker(string? text) =>
        !string.IsNullOrEmpty(text) && System.Text.RegularExpressions.Regex.IsMatch(text, @"AUTO-\d{14}-[0-9A-F]{4}");

    public static string ShortSuffix() => Guid.NewGuid().ToString("N")[..8];
}
