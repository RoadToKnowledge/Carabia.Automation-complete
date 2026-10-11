using Carabia.Automation.Framework.Base;
using OpenQA.Selenium;

namespace Carabia.Automation.Framework.Pages.Public;

/// <summary>SystemController.Sitemap -> Views/System/Sitemap.cshtml (/sitemap). XML version: /sitemap_xml.</summary>
public sealed class SitemapPage(BrowserSession session) : PageBase(session)
{
    private static readonly By TitleSelector = By.CssSelector("h1.about__title");
    private static readonly By Links = By.CssSelector(".page__content a[href]");

    protected override string Path => "/sitemap";

    public string TitleText => Session.Visible(TitleSelector, "the 'Site Map' title").Text.Trim();

    public IReadOnlyList<(string Text, string Href)> LinkData() =>
        Driver.FindElements(Links)
            .Select(a => (a.Text.Trim(), a.GetDomProperty("href") ?? string.Empty))
            .Where(l => l.Item2.Length > 0)
            .ToList();
}
