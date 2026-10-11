using Carabia.Automation.Framework.Pages.Public;
using Carabia.Automation.Framework.Utilities;
using Carabia.Automation.Tests.Base;

namespace Carabia.Automation.Tests.Search;

/// <summary>Result pagination (.product-grid__pagination, routes /sellings/page-N; page-1 is not a valid route).</summary>
[TestFixture]
[Category(TestCategories.Public)]
[Category(TestCategories.Search)]
[Category(TestCategories.Regression)]
[Parallelizable(ParallelScope.Self)]
public sealed class PaginationTests : UiTestBase
{
    [Test]
    public void First_Page_Marks_Page_One_Active_When_Paginated()
    {
        var page = new SearchPage(Session);
        page.Open();
        if (!page.Pagination.IsPresent || page.Pagination.Links().Count == 0)
            Assert.Inconclusive("All results fit on one page on this environment.");
        Assert.That(page.Pagination.ActivePage, Is.EqualTo(1));
    }

    [Test]
    public void Second_Page_Shows_Different_Cars()
    {
        var page = new SearchPage(Session);
        page.Open();
        var link = page.Pagination.Links().FirstOrDefault(l => l.Page == 2);
        if (link == default) Assert.Inconclusive("There is no second results page.");
        var firstPageUrls = page.Cards().Select(c => c.Href).ToHashSet();
        page.Pagination.ClickPage(2);
        Session.WaitFor("page 2 to become active", _ => page.Pagination.ActivePage == 2, Session.LongTimeout);
        var secondPageUrls = page.Filters.MatchingCards().Select(c => c.Url).ToList();
        Assert.Multiple(() =>
        {
            Assert.That(secondPageUrls, Is.Not.Empty);
            Assert.That(secondPageUrls.Intersect(firstPageUrls), Is.Empty, "Page 2 repeats cars from page 1.");
        });
    }

    [Test]
    public async Task Page_Route_Responds_And_Page_One_Route_Is_Not_Routable()
    {
        using var http = NewHttpProbe();
        var page2 = await http.GetAsync("/sellings/page-2");
        var page1 = await http.GetAsync("/sellings/page-1");
        Assert.Multiple(() =>
        {
            // PageNumberRouteConstraint only accepts page-N with N >= 2; page-2 must never be a server error.
            Assert.That(page2.Code, Is.InRange(200, 499), page2.ToString());
            Assert.That(page1.Code, Is.EqualTo(404), page1.ToString());
        });
    }

    [Test]
    public void Pagination_Links_Use_Page_Routes()
    {
        var page = new SearchPage(Session);
        page.Open();
        var links = page.Pagination.Links();
        if (links.Count == 0) Assert.Inconclusive("No pagination on this environment.");
        var wrong = links.Where(l => l.Page > 1 && !UrlHelper.PathOf(l.Href).EndsWith($"/page-{l.Page}")).Select(l => l.Href).ToList();
        Assert.That(wrong, Is.Empty, string.Join("\n", wrong));
    }
}
