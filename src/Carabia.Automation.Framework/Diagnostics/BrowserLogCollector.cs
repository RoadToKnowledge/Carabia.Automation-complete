using OpenQA.Selenium;

namespace Carabia.Automation.Framework.Diagnostics;

/// <summary>Collects the browser console (Chrome/Edge). Returns an empty list where unsupported.</summary>
public static class BrowserLogCollector
{
    public static IReadOnlyList<string> Collect(IWebDriver driver, LogLevel minimum = LogLevel.Warning)
    {
        try
        {
            return driver.Manage().Logs.GetLog(LogType.Browser)
                .Where(entry => entry.Level >= minimum)
                .Select(entry => $"{entry.Timestamp:HH:mm:ss} [{entry.Level}] {entry.Message}")
                .ToList();
        }
        catch (Exception exception) when (exception is WebDriverException or NotImplementedException or NullReferenceException)
        {
            return [];
        }
    }

    public static IReadOnlyList<string> SevereErrors(IWebDriver driver) => Collect(driver, LogLevel.Severe);
}
