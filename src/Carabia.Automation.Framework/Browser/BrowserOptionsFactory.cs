using Carabia.Automation.Framework.Configuration;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Chromium;
using OpenQA.Selenium.Edge;

namespace Carabia.Automation.Framework.Browser;

public static class BrowserOptionsFactory
{
    public static DriverOptions Create(BrowserType browserType, ExecutionSettings execution, string downloadDirectory, bool forceVisible = false)
    {
        ChromiumOptions options = browserType == BrowserType.Edge ? new EdgeOptions() : new ChromeOptions();
        var headless = execution.Headless && !forceVisible;

        if (headless)
            options.AddArgument("--headless=new");

        options.AddArgument($"--window-size={execution.WindowWidth},{execution.WindowHeight}");
        options.AddArgument("--no-sandbox");
        options.AddArgument("--disable-dev-shm-usage");
        options.AddArgument("--disable-notifications");
        options.AddArgument("--no-first-run");
        options.AddArgument("--disable-search-engine-choice-screen");
        options.AcceptInsecureCertificates = true;
        options.PageLoadStrategy = PageLoadStrategy.Normal;

        // Browser console messages are collected into failure artifacts.
        options.SetLoggingPreference(LogType.Browser, LogLevel.All);

        Directory.CreateDirectory(downloadDirectory);
        options.AddUserProfilePreference("download.default_directory", downloadDirectory);
        options.AddUserProfilePreference("download.prompt_for_download", false);

        // Optional explicit binary (for example a portable Chrome on a CI agent).
        var binary = Environment.GetEnvironmentVariable("CARABIA_BROWSER_BINARY");
        if (!string.IsNullOrWhiteSpace(binary))
            options.BinaryLocation = binary;

        return options;
    }
}
