using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace Carabia.Automation.Framework.Utilities;

/// <summary>Small, explicit-wait based element operations shared by page objects.</summary>
public sealed class ElementActions(IWebDriver driver, TimeSpan timeout)
{
    public IWebElement Visible(By by, string? description = null) =>
        WaitHelper.ForVisible(driver, by, description ?? $"element {by} to be visible", timeout);

    public IWebElement Clickable(By by, string? description = null) =>
        WaitHelper.ForClickable(driver, by, description ?? $"element {by} to be clickable", timeout);

    public bool Exists(By by) => driver.FindElements(by).Count > 0;

    public bool IsDisplayed(By by)
    {
        try
        {
            return driver.FindElements(by).Any(element => element.Displayed);
        }
        catch (WebDriverException)
        {
            return false;
        }
    }

    public IReadOnlyList<IWebElement> All(By by) => driver.FindElements(by);

    public IReadOnlyList<IWebElement> AllVisible(By by) => driver.FindElements(by).Where(e => e.Displayed).ToList();

    public void Click(By by, string? description = null) => Click(Clickable(by, description));

    /// <summary>Scrolls into view first; falls back to a JavaScript click if a sticky header covers the element.</summary>
    public void Click(IWebElement element)
    {
        JavaScriptHelper.ScrollIntoView(driver, element);
        try
        {
            element.Click();
        }
        catch (ElementClickInterceptedException)
        {
            JavaScriptHelper.Click(driver, element);
        }
    }

    public void Type(By by, string value, bool clearFirst = true, bool tabOut = false)
    {
        var input = Visible(by);
        if (clearFirst)
        {
            input.SendKeys(Keys.Control + "a");
            input.SendKeys(Keys.Delete);
        }
        input.SendKeys(value);
        if (tabOut) input.SendKeys(Keys.Tab);
    }

    public string Text(By by) => Visible(by).Text.Trim();

    public string Value(By by) => driver.FindElement(by).GetDomProperty("value") ?? string.Empty;

    public SelectElement Select(By by) => new(driver.FindElement(by));
}
