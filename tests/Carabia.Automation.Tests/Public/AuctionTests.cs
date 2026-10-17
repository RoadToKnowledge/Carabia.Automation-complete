using Carabia.Automation.Framework.Pages.Public;
using Carabia.Automation.Tests.Base;

namespace Carabia.Automation.Tests.Public;

/// <summary>Auctions page: heading, coming-soon vs. visible grid, sold/upcoming redirects, detail page.</summary>
[TestFixture]
[Category(TestCategories.Public)]
[Category(TestCategories.Regression)]
[Parallelizable(ParallelScope.Self)]
public sealed class AuctionTests : UiTestBase
{
    [Test]
    [Category(TestCategories.Auctions)]
    public void Auctions_Page_Has_Heading_And_Intro_Text()
    {
        var page = new AuctionsPage(Session);
        page.Open();
        Assert.Multiple(() =>
        {
            Assert.That(page.HeadingText, Is.Not.Empty, "Auctions page must have a heading.");
            Assert.That(page.IntroTextValue, Is.Not.Empty, "Auctions page must have intro text.");
        });
    }

    [Test]
    [Category(TestCategories.Auctions)]
    public void Auctions_Visible_Mode_Shows_Grid_And_Filters()
    {
        var page = new AuctionsPage(Session);
        page.OpenWithAuctionsVisible();
        Assert.Multiple(() =>
        {
            Assert.That(page.HasAuctionGrid, Is.True, "Visible mode must show auction grid.");
            Assert.That(page.HasFilters, Is.True, "Visible mode must show filters.");
        });
    }

    [Test]
    [Category(TestCategories.Auctions)]
    [Category(TestCategories.Http)]
    public async Task Auctions_Sold_And_Upcoming_Redirect_To_Main()
    {
        using var http = NewHttpProbe();
        foreach (var path in new[] { "/auctions/sold", "/auctions/upcoming" })
        {
            var response = await http.GetAsync(path, followRedirects: false);
            Assert.That(response.IsRedirect, Is.True,
                $"{path} should redirect (got {response.Code}).");
            Assert.That(response.Location?.ToString() ?? "", Does.Contain("/auctions"),
                $"{path} should redirect to /auctions.");
        }
    }

    [Test]
    [Category(TestCategories.Auctions)]
    public void Auction_Detail_Page_Opens_If_Auctions_Exist()
    {
        var page = new AuctionsPage(Session);
        page.OpenWithAuctionsVisible();
        var links = page.AuctionLinks();
        if (links.Count == 0) Assert.Inconclusive("No auction listings to open.");

        var detail = new AuctionDetailsPage(Session);
        detail.OpenAuction(links.First());
        Assert.Multiple(() =>
        {
            Assert.That(detail.HeadingText, Is.Not.Empty, "Auction detail must have a heading.");
            Assert.That(detail.SectionTitleTexts().Count, Is.GreaterThan(0),
                "Auction detail must have section titles.");
        });
    }
}
