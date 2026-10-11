using Carabia.Automation.Framework.Authentication;
using Carabia.Automation.Framework.Browser;
using Carabia.Automation.Framework.Configuration;
using Carabia.Automation.Framework.Utilities;
using OpenQA.Selenium;

namespace Carabia.Automation.Framework.Base;

/// <summary>
/// One browser plus everything page objects need: settings, waits, step banner and logging.
/// Equivalent of carabia-ui-tests' BrowserFixture, but configuration driven.
/// </summary>
public sealed class BrowserSession : IDisposable
{
    private bool _disposed;

    public BrowserSession(TestSettings settings, IWebDriver driver, SessionRole role, Action<string>? log = null)
    {
        Settings = settings;
        Driver = driver;
        Role = role;
        Log = log ?? (_ => { });
        Elements = new ElementActions(driver, DefaultTimeout);
    }

    public static BrowserSession Start(TestSettings settings, SessionRole role, Action<string>? log = null, bool forceVisible = false) =>
        new(settings, DriverFactory.Create(settings, forceVisible), role, log);

    public TestSettings Settings { get; }
    public IWebDriver Driver { get; }
    public SessionRole Role { get; }
    public ElementActions Elements { get; }
    public Action<string> Log { get; set; }

    public string BaseUrl => Settings.BaseUrl;
    public TimeSpan DefaultTimeout => TimeSpan.FromSeconds(Settings.Execution.DefaultTimeoutSeconds);
    public TimeSpan LongTimeout => TimeSpan.FromSeconds(Math.Max(60, Settings.Execution.DefaultTimeoutSeconds * 3));
    public string BrowserName => DriverFactory.ResolveBrowser(Settings.Execution).ToString();
    public string CurrentUrl => SafeUrl();

    public Uri Url(string relativePath) => UrlHelper.Combine(BaseUrl, relativePath);

    /// <summary>Opens a page relative to Execution.BaseUrl and waits for document.readyState == complete.</summary>
    public void Open(string relativePath)
    {
        var url = Url(relativePath);
        try
        {
            Driver.Navigate().GoToUrl(url);
        }
        catch (WebDriverException exception)
        {
            throw new AutomationException(
                $"Could not open {url}. Check that Carabia is reachable at {BaseUrl}. WebDriver: {exception.Message}", exception);
        }

        WaitHelper.ForDocumentReady(Driver, TimeSpan.FromSeconds(Settings.Execution.PageLoadTimeoutSeconds));
    }

    public T WaitFor<T>(string expected, Func<IWebDriver, T> condition, TimeSpan? timeout = null) =>
        WaitHelper.Until(Driver, expected, condition, timeout ?? DefaultTimeout);

    public IWebElement Visible(By selector, string description, TimeSpan? timeout = null) =>
        WaitHelper.ForVisible(Driver, selector, description, timeout ?? DefaultTimeout);

    /// <summary>Writes the step to the test output and, for visible runs, shows it in the browser.</summary>
    public void Step(string message, IWebElement? element = null)
    {
        Log(message);
        if (!Settings.Execution.ShowStepBanner) return;
        try
        {
            ((IJavaScriptExecutor)Driver).ExecuteScript("""
                let label = document.getElementById('carabia-test-progress');
                if (!label && document.body) {
                    label = document.createElement('div');
                    label.id = 'carabia-test-progress';
                    label.style.cssText = 'position:fixed;bottom:16px;left:16px;z-index:2147483647;max-width:520px;padding:12px 18px;background:#17375e;color:white;font:15px/1.5 sans-serif;border-radius:8px;pointer-events:none;box-shadow:0 2px 12px #0006';
                    document.body.appendChild(label);
                }
                if (label) label.textContent = 'TEST: ' + arguments[0];
                if (arguments[1]) arguments[1].scrollIntoView({block:'center', behavior:'instant'});
                """, message, element);
        }
        catch (WebDriverException)
        {
            // The banner is cosmetic; never fail a test because the page was navigating.
        }

        PauseForPresentation();
    }

    /// <summary>The only intentional pause: lets a person follow a visible walkthrough. 0 in headless runs.</summary>
    public void PauseForPresentation()
    {
        var delay = Settings.Execution.Headless ? 0 : Settings.Execution.PresentationDelayMs;
        if (delay > 0)
            Thread.Sleep(Math.Min(delay, 10_000));
    }

    public AuthStatus AuthStatus() => AuthStatusProbe.Read(Driver);

    public void ResetToAnonymous()
    {
        // Clean state without automated logout: drop cookies and storage for the Carabia origin.
        Open("/");
        Driver.Manage().Cookies.DeleteAllCookies();
        ((IJavaScriptExecutor)Driver).ExecuteScript("try { localStorage.clear(); sessionStorage.clear(); } catch (e) {}");
    }

    private string SafeUrl()
    {
        try
        {
            return Driver.Url;
        }
        catch (WebDriverException)
        {
            return "(browser unavailable)";
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        try
        {
            Driver.Quit();
        }
        catch (WebDriverException)
        {
        }
        Driver.Dispose();
    }
}
