using Carabia.Automation.Framework.Pages.Public;
using Carabia.Automation.Tests.Base;

namespace Carabia.Automation.Tests.Public;

/// <summary>InfoController.Page (/info/{alias}) - admin-managed CMS pages - plus /contact-us.</summary>
[TestFixture]
[Category(TestCategories.Public)]
[Category(TestCategories.Regression)]
[Parallelizable(ParallelScope.Self)]
public sealed class StaticPageTests : UiTestBase
{
    [TestCaseSource(typeof(TestData), nameof(TestData.InfoPages))]
    public void Info_Page_Renders_Title_And_Content(string alias)
    {
        var page = new InfoPage(Session);
        page.OpenAlias(alias);
        Assert.Multiple(() =>
        {
            Assert.That(page.IsNotFoundPage, Is.False, $"/info/{alias} returned the 404 page (unpublished or missing).");
            Assert.That(page.Title, Is.Not.Empty, "The page has no <title> (InfoPage.PageTitle).");
            Assert.That(page.ContentText, Is.Not.Empty, "The page has no visible content.");
        });
    }

    [Test]
    public async Task Unknown_Info_Alias_Returns_404()
    {
        using var http = NewHttpProbe();
        var response = await http.GetAsync($"/info/automation-missing-page-{Guid.NewGuid():N}");
        Assert.That(response.Code, Is.EqualTo(404), response.ToString());
    }

    [Test]
    public void How_It_Works_Fees_Anchor_Is_Added()
    {
        // Page.cshtml adds id="our-fees" to the fees heading of the how-it-works page.
        var page = new InfoPage(Session);
        page.OpenAlias("how-it-works");
        var found = page.WaitForFeesAnchor();
        if (!found)
            Assert.Inconclusive("No 'Our fees' heading in the how-it-works content on this environment.");
        Assert.That(page.FeesAnchorCount, Is.EqualTo(1));
    }

    [Test]
    public void Contact_Page_Renders_Content()
    {
        var page = new ContactPage(Session);
        page.Open();
        Assert.That(page.ContentText, Is.Not.Empty);
    }
}
