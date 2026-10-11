using OpenQA.Selenium;

namespace Carabia.Automation.Framework.Authentication;

/// <summary>Reads and restores window.localStorage / window.sessionStorage for the current origin.</summary>
public static class LocalStorageManager
{
    public static Dictionary<string, string> ReadAll(IWebDriver driver, string storage = "localStorage")
    {
        var raw = ((IJavaScriptExecutor)driver).ExecuteScript($$"""
            const result = {};
            try {
              const s = window.{{storage}};
              for (let i = 0; i < s.length; i++) { const k = s.key(i); result[k] = s.getItem(k); }
            } catch (e) {}
            return result;
            """);

        return raw is IDictionary<string, object> dictionary
            ? dictionary.ToDictionary(x => x.Key, x => x.Value?.ToString() ?? string.Empty, StringComparer.Ordinal)
            : new Dictionary<string, string>(StringComparer.Ordinal);
    }

    public static void Restore(IWebDriver driver, IReadOnlyDictionary<string, string> values, string storage = "localStorage")
    {
        foreach (var item in values)
        {
            ((IJavaScriptExecutor)driver).ExecuteScript(
                $"try {{ window.{storage}.setItem(arguments[0], arguments[1]); }} catch (e) {{}}", item.Key, item.Value);
        }
    }
}
