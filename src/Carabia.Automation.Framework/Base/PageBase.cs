using Carabia.Automation.Framework.Utilities;
using OpenQA.Selenium;

namespace Carabia.Automation.Framework.Base;

/// <summary>Base for all page objects. Selectors live in the page objects, never in tests.</summary>
public abstract class PageBase(BrowserSession session) : ComponentBase(session)
{
    private static readonly By CanonicalLink = By.CssSelector("link[rel='canonical']");
    private static readonly By MainHeading = By.TagName("h1");

    /// <summary>Route of the page relative to BaseUrl, when it has a fixed one.</summary>
    protected virtual string? Path => null;

    public virtual void Open()
    {
        if (Path is null)
            throw new InvalidOperationException($"{GetType().Name} has no fixed route; use a page-specific open method.");
        Session.Open(Path);
    }

    public void OpenPath(string relativePath) => Session.Open(relativePath);

    public string Title => Driver.Title?.Trim() ?? string.Empty;
    public string CurrentPath => UrlHelper.PathOf(Driver.Url);

    public string? CanonicalHref => Driver.FindElements(CanonicalLink).FirstOrDefault()?.GetDomAttribute("href");

    public string? MetaDescription => JavaScriptHelper.Meta(Driver, "meta[name='description']", "content");

    public string FirstH1Text => Driver.FindElements(MainHeading).FirstOrDefault(e => e.Displayed)?.Text.Trim() ?? string.Empty;

    public string BodyText => Driver.FindElement(By.TagName("body")).Text;

    public bool ShowsBrowserErrorPage =>
        BodyText.Contains("ERR_CONNECTION", StringComparison.OrdinalIgnoreCase)
        || BodyText.Contains("This site can’t be reached", StringComparison.OrdinalIgnoreCase);

    /// <summary>Carabia's 404 view (Views/System/NotFoundPage.cshtml) renders #notfound.</summary>
    public bool IsNotFoundPage => Elements.Exists(By.Id("notfound"));

    private static readonly By MainContent = By.CssSelector(".page__content");
    private static readonly By Links = By.CssSelector("a[href]");
    private static readonly By Images = By.CssSelector("img[src]");

    /// <summary>Text of the shared layout's main content wrapper (.page__content in Views/Shared/_Layout.cshtml).</summary>
    public string MainContentText => Session.Visible(MainContent, "the page content (.page__content)").Text.Trim();

    /// <summary>Distinct same-host link targets on the page (fragments removed, logout excluded).</summary>
    public IReadOnlyList<string> InternalLinkHrefs(int max = 150) =>
        Driver.FindElements(Links)
            .Select(a => a.GetDomProperty("href") ?? string.Empty)
            .Where(h => UrlHelper.IsSameHost(h, Session.BaseUrl))
            .Select(h => h.Split('#')[0])
            .Where(h => !h.Contains("/identity/logout", StringComparison.OrdinalIgnoreCase))
            .Distinct()
            .Take(max)
            .ToList();

    /// <summary>Visible, non-lazy images that failed to load.</summary>
    public IReadOnlyList<string> BrokenVisibleImages() =>
        Driver.FindElements(Images)
            .Where(i => i.Displayed && (i.GetDomAttribute("loading") ?? "") != "lazy")
            .Where(i => !JavaScriptHelper.IsImageLoaded(Driver, i))
            .Select(i => i.GetDomProperty("src") ?? string.Empty)
            .Distinct()
            .ToList();

    /// <summary>Clicks a link and waits until the old document is gone (also catches reloads of the same URL).</summary>
    public void ClickAndWaitForNavigation(IWebElement link, string description)
    {
        var html = Driver.FindElement(By.TagName("html"));
        Elements.Click(link);
        WaitHelper.ForStale(Driver, html, description, Session.LongTimeout);
        WaitHelper.ForDocumentReady(Driver, Session.LongTimeout);
    }

    /// <summary>True if any personal-area content (side navigation or settings form) was rendered.</summary>
    public bool RendersPersonalContent =>
        Elements.Exists(By.CssSelector(".content__nav")) || Elements.Exists(By.CssSelector("form.settings"));
}
