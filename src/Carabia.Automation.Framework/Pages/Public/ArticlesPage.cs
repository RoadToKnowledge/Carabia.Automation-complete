using Carabia.Automation.Framework.Base;
using OpenQA.Selenium;

namespace Carabia.Automation.Framework.Pages.Public;

/// <summary>
/// ArticlesController.Index -> Views/Articles/Index.cshtml. Route "{category}" is constrained to existing
/// NewsCategory titles (lower case, spaces -> '-'), e.g. /news, /overviews, /buying-guides, /selling-guides, /partners.
/// </summary>
public sealed class ArticlesPage(BrowserSession session) : PageBase(session)
{
    private static readonly By Heading = By.CssSelector(".crb-news-page-content .section__title h1");
    private static readonly By CategoryButtons = By.CssSelector(".crb-news-page-content a.button.button_size_xs");
    private static readonly By ArticleLinks = By.CssSelector(".article-grid__list a.article-snippet__illustration");
    private static readonly By ArticleTitles = By.CssSelector(".article-grid__list .article-snippet__title");

    public void OpenCategory(string categorySlug) => Session.Open($"/{categorySlug.Trim('/')}");

    public string HeadingText => Session.Visible(Heading, "the article category heading").Text.Trim();

    public IReadOnlyList<(string Text, string Href)> Categories() =>
        Driver.FindElements(CategoryButtons).Select(a => (a.Text.Trim(), a.GetDomProperty("href") ?? string.Empty)).ToList();

    public IReadOnlyList<string> ArticleHrefs() =>
        Driver.FindElements(ArticleLinks).Select(a => a.GetDomProperty("href") ?? string.Empty).Where(h => h.Length > 0).Distinct().ToList();

    public IReadOnlyList<string> ArticleTitleTexts() =>
        Driver.FindElements(ArticleTitles).Select(t => t.Text.Trim()).Where(t => t.Length > 0).ToList();
}

/// <summary>ArticlesController.Article -> Views/Articles/Article.cshtml, route /news/{alias} (aliases are normalised with a 301).</summary>
public sealed class ArticleDetailsPage(BrowserSession session) : PageBase(session)
{
    private static readonly By TitleSelector = By.CssSelector("h1.article__title");
    private static readonly By Content = By.CssSelector(".article__content");
    private static readonly By Date = By.CssSelector(".article__meta .article__data");
    private static readonly By Image = By.CssSelector(".article__illustration img.article__image");
    private static readonly By FeaturedListings = By.CssSelector(".listing-card");

    public void OpenArticle(string url) => Session.Open(Utilities.UrlHelper.RouteOf(url));

    public string TitleText => Session.Visible(TitleSelector, "the article title").Text.Trim();
    public string ContentText => Driver.FindElements(Content).FirstOrDefault()?.Text.Trim() ?? string.Empty;
    public string DateText => Driver.FindElements(Date).FirstOrDefault()?.Text.Trim() ?? string.Empty;
    public IWebElement? ImageElement => Driver.FindElements(Image).FirstOrDefault();
    public int FeaturedListingCount => Driver.FindElements(FeaturedListings).Count;
}
