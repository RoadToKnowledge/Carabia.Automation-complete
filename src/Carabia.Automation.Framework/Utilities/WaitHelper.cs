using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace Carabia.Automation.Framework.Utilities;

/// <summary>
/// All synchronisation goes through here. There is no Thread.Sleep-based waiting in the framework;
/// the only deliberate pause is the optional presentation delay for visible walkthroughs.
/// </summary>
public static class WaitHelper
{
    public static WebDriverWait Create(IWebDriver driver, TimeSpan timeout, TimeSpan? polling = null)
    {
        var wait = new WebDriverWait(new SystemClock(), driver, timeout, polling ?? TimeSpan.FromMilliseconds(250));
        wait.IgnoreExceptionTypes(typeof(NoSuchElementException), typeof(StaleElementReferenceException),
            typeof(ElementNotInteractableException));
        return wait;
    }

    /// <summary>Waits for <paramref name="condition"/> and turns a timeout into a readable message.</summary>
    public static T Until<T>(IWebDriver driver, string expected, Func<IWebDriver, T> condition, TimeSpan timeout, TimeSpan? polling = null)
    {
        try
        {
            return Create(driver, timeout, polling).Until(condition);
        }
        catch (WebDriverTimeoutException)
        {
            string url;
            try { url = driver.Url; } catch (WebDriverException) { url = "(unavailable)"; }
            throw new AutomationException($"Expected {expected}, but it did not happen within {timeout.TotalSeconds:0} seconds. Page: {url}");
        }
    }

    public static bool TryUntil(IWebDriver driver, Func<IWebDriver, bool> condition, TimeSpan timeout)
    {
        try
        {
            return Create(driver, timeout).Until(condition);
        }
        catch (WebDriverTimeoutException)
        {
            return false;
        }
    }

    public static void ForDocumentReady(IWebDriver driver, TimeSpan timeout) =>
        Until(driver, "the page to finish loading", d =>
            ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState")?.ToString() == "complete", timeout);

    /// <summary>Admin pages use jQuery; public pages mostly use fetch. Waits for jQuery AJAX when jQuery exists.</summary>
    public static void ForJQueryIdle(IWebDriver driver, TimeSpan timeout) =>
        Until(driver, "jQuery AJAX requests to finish", d =>
            ((IJavaScriptExecutor)d).ExecuteScript("return !window.jQuery || window.jQuery.active === 0;") is true, timeout);

    public static IWebElement ForVisible(IWebDriver driver, By by, string description, TimeSpan timeout) =>
        Until(driver, description, d =>
        {
            var element = d.FindElements(by).FirstOrDefault(e => e.Displayed);
            return element;
        }, timeout)!;

    public static IWebElement ForClickable(IWebDriver driver, By by, string description, TimeSpan timeout) =>
        Until(driver, description, d =>
        {
            var element = d.FindElements(by).FirstOrDefault(e => e.Displayed && e.Enabled);
            return element;
        }, timeout)!;

    public static void ForInvisible(IWebDriver driver, By by, string description, TimeSpan timeout) =>
        Until(driver, description, d => d.FindElements(by).All(e => !e.Displayed), timeout);

    public static void ForStale(IWebDriver driver, IWebElement element, string description, TimeSpan timeout) =>
        Until(driver, description, d =>
        {
            try
            {
                _ = element.TagName;
                return false;
            }
            catch (StaleElementReferenceException)
            {
                return true;
            }
        }, timeout);

    public static void ForUrl(IWebDriver driver, Func<Uri, bool> predicate, string description, TimeSpan timeout) =>
        Until(driver, description, d => Uri.TryCreate(d.Url, UriKind.Absolute, out var uri) && predicate(uri), timeout);

    /// <summary>A select element is "loaded" when it is enabled and offers more than its placeholder option.</summary>
    public static SelectElement ForSelectOptions(IWebDriver driver, By by, string description, TimeSpan timeout, int minimumOptions = 2) =>
        Until(driver, description, d =>
        {
            var element = d.FindElement(by);
            if (!element.Enabled) return null;
            var select = new SelectElement(element);
            return select.Options.Count >= minimumOptions
                   && !select.Options.Any(o => o.Text.Contains("Loading", StringComparison.OrdinalIgnoreCase))
                ? select
                : null;
        }, timeout)!;
}

/// <summary>Framework failure with a message written for the person reading the test result.</summary>
public sealed class AutomationException(string message, Exception? inner = null) : Exception(message, inner);
