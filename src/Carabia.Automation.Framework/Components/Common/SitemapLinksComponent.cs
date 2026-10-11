using Carabia.Automation.Framework.Base;
using OpenQA.Selenium;

namespace Carabia.Automation.Framework.Components.Common;

/// <summary>Views/Shared/Partial/_SitemapLinksPartial.cshtml (".crb-sitemap-section"), rendered on home, listings and brand pages.</summary>
public sealed class SitemapLinksComponent(BrowserSession session) : ComponentBase(session)
{
    public static readonly string[] Groups =
        ["Trending Searches", "Popular Locations", "Popular Makes", "Popular Budgets", "Popular Body Types", "Popular Models"];

    private static readonly By Section = By.CssSelector(".crb-sitemap-section");
    private static readonly By ColumnTitle = By.CssSelector(".crb-sitemap-col-title");
    private static readonly By Column = By.CssSelector(".crb-sitemap-col");
    private static readonly By Link = By.CssSelector("a.crb-sitemap-link");

    public IWebElement Element => Session.Visible(Section, "the sitemap links section");

    public IReadOnlyList<string> GroupTitles() =>
        Element.FindElements(ColumnTitle).Select(e => e.Text.Trim()).ToList();

    public IReadOnlyList<IWebElement> LinksIn(string group) =>
        Element.FindElements(Column)
            .Single(column => column.FindElement(ColumnTitle).Text.Trim() == group)
            .FindElements(Link);

    public IReadOnlyList<(string Label, string Href)> LinkDataIn(string group) =>
        LinksIn(group).Select(a => (Label: a.Text.Trim(), Href: a.GetDomProperty("href") ?? string.Empty)).ToList();
}
