using Carabia.Automation.Framework.Configuration;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;

namespace Carabia.Automation.Framework.Browser;

/// <summary>
/// Creates a fresh WebDriver. A fresh driver is also how tests get a clean, logged-out state:
/// the framework never clicks Logout.
/// </summary>
public static class DriverFactory
{
    public static BrowserType ResolveBrowser(ExecutionSettings execution) =>
        Enum.TryParse<BrowserType>(execution.Browser, ignoreCase: true, out var parsed) ? parsed : BrowserType.Chrome;

    public static IWebDriver Create(TestSettings settings, bool forceVisible = false)
    {
        var execution = settings.Execution;
        var browserType = ResolveBrowser(execution);
        var downloads = settings.ResolvePath(Path.Combine(execution.ArtifactsDirectory, "downloads"));
        var options = BrowserOptionsFactory.Create(browserType, execution, downloads, forceVisible);

        // An explicit driver path can be supplied when Selenium Manager cannot download (offline agents).
        var driverPath = Environment.GetEnvironmentVariable("CARABIA_DRIVER_PATH");

        IWebDriver driver;
        try
        {
            driver = browserType switch
            {
                BrowserType.Edge => string.IsNullOrWhiteSpace(driverPath)
                    ? new EdgeDriver((EdgeOptions)options)
                    : new EdgeDriver(EdgeDriverService.CreateDefaultService(driverPath), (EdgeOptions)options),
                _ => string.IsNullOrWhiteSpace(driverPath)
                    ? new ChromeDriver((ChromeOptions)options)
                    : new ChromeDriver(ChromeDriverService.CreateDefaultService(driverPath), (ChromeOptions)options)
            };
        }
        catch (WebDriverException exception)
        {
            throw new InvalidOperationException(
                $"Could not start {browserType}. Make sure {browserType} is installed; Selenium Manager downloads the driver automatically " +
                $"(set CARABIA_DRIVER_PATH to a driver folder if this machine is offline). WebDriver: {exception.Message}", exception);
        }

        var timeouts = driver.Manage().Timeouts();
        timeouts.PageLoad = TimeSpan.FromSeconds(execution.PageLoadTimeoutSeconds);
        timeouts.AsynchronousJavaScript = TimeSpan.FromSeconds(Math.Max(30, execution.DefaultTimeoutSeconds));
        timeouts.ImplicitWait = TimeSpan.Zero; // all synchronisation is explicit (WaitHelper)

        if (!execution.Headless || forceVisible)
            driver.Manage().Window.Size = new System.Drawing.Size(execution.WindowWidth, execution.WindowHeight);

        return driver;
    }
}
