using OpenQA.Selenium;

namespace Carabia.Automation.Framework.Utilities;

public static class JavaScriptHelper
{
    private static IJavaScriptExecutor Js(IWebDriver driver) => (IJavaScriptExecutor)driver;

    public static object? Execute(IWebDriver driver, string script, params object?[] args) => Js(driver).ExecuteScript(script, args);

    public static void ScrollToTop(IWebDriver driver) =>
        Js(driver).ExecuteScript("window.scrollTo({ top: 0, behavior: 'instant' });");

    public static void ScrollIntoView(IWebDriver driver, IWebElement element) =>
        Js(driver).ExecuteScript("arguments[0].scrollIntoView({block:'center', inline:'nearest', behavior:'instant'});", element);

    public static void Click(IWebDriver driver, IWebElement element) => Js(driver).ExecuteScript("arguments[0].click();", element);

    public static bool IsImageLoaded(IWebDriver driver, IWebElement image) =>
        Js(driver).ExecuteScript("return arguments[0].complete && arguments[0].naturalWidth > 0;", image) is true;

    /// <summary>Visible to the user: displayed, not hidden by CSS and with a real height.</summary>
    public static bool IsReallyVisible(IWebDriver driver, IWebElement element) =>
        element.Displayed && Js(driver).ExecuteScript("""
            const e = arguments[0]; const s = getComputedStyle(e);
            return s.display !== 'none' && s.visibility !== 'hidden' && e.getBoundingClientRect().height > 1;
            """, element) is true;

    public static string? Meta(IWebDriver driver, string cssSelector, string attribute) =>
        Js(driver).ExecuteScript("const e = document.querySelector(arguments[0]); return e ? e.getAttribute(arguments[1]) : null;",
            cssSelector, attribute) as string;

    /// <summary>Same-origin fetch from inside the page (keeps the browser's cookies). Returns the HTTP status.</summary>
    public static long FetchStatus(IWebDriver driver, string relativeUrl) =>
        Convert.ToInt64(Js(driver).ExecuteAsyncScript("""
            const [url, done] = arguments;
            fetch(url, { credentials: 'same-origin', cache: 'no-store', redirect: 'manual' })
              .then(r => done(r.status)).catch(() => done(-1));
            """, relativeUrl));

    /// <summary>Accepts the native confirm() that admin "Remove" links show. Only used by guarded destructive tests.</summary>
    public static void AutoAcceptNextConfirm(IWebDriver driver) =>
        Js(driver).ExecuteScript("window.__carabiaConfirm = window.confirm; window.confirm = function(){ return true; };");
}
