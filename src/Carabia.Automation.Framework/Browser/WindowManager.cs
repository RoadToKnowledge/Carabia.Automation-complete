using OpenQA.Selenium;

namespace Carabia.Automation.Framework.Browser;

/// <summary>Viewport and tab helpers.</summary>
public static class WindowManager
{
    public static void Resize(IWebDriver driver, Viewport viewport)
    {
        driver.Manage().Window.Size = new System.Drawing.Size(viewport.Width, viewport.Height);
    }

    /// <summary>Inner width as the page sees it (the window size includes browser chrome).</summary>
    public static long InnerWidth(IWebDriver driver) =>
        Convert.ToInt64(((IJavaScriptExecutor)driver).ExecuteScript("return window.innerWidth;"));

    /// <summary>True when the document is wider than the viewport, i.e. the page scrolls sideways.</summary>
    public static bool HasHorizontalOverflow(IWebDriver driver) =>
        ((IJavaScriptExecutor)driver).ExecuteScript(
            "return document.documentElement.scrollWidth > window.innerWidth + 1;") is true;

    /// <summary>Runs <paramref name="action"/> in a new tab opened by <paramref name="trigger"/>, then returns to the original tab.</summary>
    public static T InNewTab<T>(IWebDriver driver, Action trigger, Func<IWebDriver, T> action, TimeSpan timeout)
    {
        var original = driver.CurrentWindowHandle;
        var before = driver.WindowHandles.ToHashSet();
        trigger();
        var wait = Utilities.WaitHelper.Create(driver, timeout);
        var handle = wait.Until(d => d.WindowHandles.FirstOrDefault(h => !before.Contains(h)));
        driver.SwitchTo().Window(handle);
        try
        {
            return action(driver);
        }
        finally
        {
            driver.Close();
            driver.SwitchTo().Window(original);
        }
    }
}
