using Carabia.Automation.Framework.Pages.Public;
using Carabia.Automation.Framework.Utilities;
using Carabia.Automation.Tests.Base;

namespace Carabia.Automation.Tests.Public;

/// <summary>ArticlesController: category pages ("{category}") and article pages (/news/{alias}).</summary>
[TestFixture]
[Category(TestCategories.Public)]
[Category(TestCategories.Articles)]
[Category(TestCategories.Regression)]
[Parallelizable(ParallelScope.Self)]
public sealed class ArticleTests : UiTestBase
{
    [TestCaseSource(typeof(TestData), nameof(TestData.ArticleCategories))]
    public void Article_Category_Page_Shows_Heading_And_Articles(string category)
    {
        var page = new ArticlesPage(Session);
        page.OpenCategory(category);
        Assert.Multiple(() =>
        {
            Assert.That(page.IsNotFoundPage, Is.False, $"/{category} is not a routable news category.");
            Assert.That(page.HeadingText, Is.Not.Empty);
            Assert.That(page.ArticleHrefs(), Is.Not.Empty, $"/{category} lists no articles.");
            Assert.That(page.ArticleHrefs().All(h => UrlHelper.PathOf(h).StartsWith("/news/")), Is.True, "Article links must use /news/{alias}.");
        });
    }

    [Test]
    public void Article_Category_Buttons_Link_To_Other_Categories()
    {
        var page = new ArticlesPage(Session);
        page.OpenCategory("news");
        var categories = page.Categories();
        Assert.That(categories, Is.Not.Empty, "No category buttons on the news page.");
        using var http = NewHttpProbe();
        var failures = categories.Select(c => http.GetAsync(c.Href).GetAwaiter().GetResult()).Where(r => !r.IsSuccess).Select(r => r.ToString()).ToList();
        Assert.That(failures, Is.Empty, string.Join("\n", failures));
    }

    [Test]
    public void Opening_An_Article_Shows_Title_Date_And_Content()
    {
        var list = new ArticlesPage(Session);
        list.OpenCategory("news");
        var href = list.ArticleHrefs().FirstOrDefault() ?? throw new InconclusiveException("No news articles on this environment.");
        var article = new ArticleDetailsPage(Session);
        article.OpenArticle(href);
        Assert.Multiple(() =>
        {
            Assert.That(article.TitleText, Is.Not.Empty);
            Assert.That(article.DateText, Is.Not.Empty);
            Assert.That(article.ContentText, Is.Not.Empty);
            Assert.That(article.CanonicalHref is null || UrlHelper.PathOf(article.CanonicalHref).StartsWith("/news/"), Is.True);
        });
    }

    [Test]
    public async Task Article_Alias_With_Upper_Case_Redirects_To_Lower_Case()
    {
        var list = new ArticlesPage(Session);
        list.OpenCategory("news");
        var href = list.ArticleHrefs().FirstOrDefault() ?? throw new InconclusiveException("No news articles on this environment.");
        var path = UrlHelper.PathOf(href);
        using var http = NewHttpProbe();
        var response = await http.GetAsync(path.ToUpperInvariant().Replace("/NEWS/", "/news/"), followRedirects: false);
        Assert.That(response.IsRedirect, Is.True, $"Expected a permanent redirect to the normalised alias: {response}");
        Assert.That(UrlHelper.PathOf(response.Location?.IsAbsoluteUri == true ? response.Location.AbsoluteUri : http.Absolute(response.Location?.OriginalString ?? "").AbsoluteUri),
            Is.EqualTo(path).IgnoreCase);
    }

    [Test]
    public async Task Unknown_Article_Returns_404()
    {
        using var http = NewHttpProbe();
        var response = await http.GetAsync($"/news/automation-missing-{Guid.NewGuid():N}");
        Assert.That(response.Code, Is.EqualTo(404), response.ToString());
    }
}
