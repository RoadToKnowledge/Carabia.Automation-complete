using System.Globalization;
using System.Text.RegularExpressions;

namespace Carabia.Automation.Framework.Utilities;

public static class TextParser
{
    /// <summary>First number in the text, e.g. "385 Results" -> 385, "AED 40,000" -> 40000.</summary>
    public static decimal Number(string text)
    {
        var match = Regex.Match(text ?? string.Empty, @"\d[\d,]*(?:\.\d+)?");
        if (!match.Success)
            throw new AutomationException($"Expected a number but saw '{text}'.");
        return decimal.Parse(match.Value, NumberStyles.Number, CultureInfo.InvariantCulture);
    }

    public static bool TryNumber(string text, out decimal value)
    {
        var match = Regex.Match(text ?? string.Empty, @"\d[\d,]*(?:\.\d+)?");
        value = match.Success ? decimal.Parse(match.Value, NumberStyles.Number, CultureInfo.InvariantCulture) : 0;
        return match.Success;
    }

    /// <summary>All digits joined, e.g. "AED 40,000.00" -> 40000.00</summary>
    public static decimal Digits(string text) =>
        decimal.Parse(Regex.Replace(text ?? "0", @"[^\d.]", "") is { Length: > 0 } d ? d : "0", CultureInfo.InvariantCulture);

    public static string Normalize(string value) => Regex.Replace(value ?? string.Empty, @"\s+", " ").Trim();
}
