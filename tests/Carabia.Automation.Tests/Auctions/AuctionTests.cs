using Carabia.Automation.Framework.Pages.Public;
using Carabia.Automation.Framework.Utilities;
using Carabia.Automation.Tests.Base;

namespace Carabia.Automation.Tests.Auctions;

/// <summary>
/// AuctionsController (public). Auctions are switched off in the current release: /auctions shows "coming soon"
/// unless ?auctions_visible is present, and /auctions/sold|upcoming redirect to /auctions. Bidding (AddBid) is a
/// write on live auctions and is not automated (documented in the coverage matrix).
/// </summary>
[TestFixture]
[Category(TestCategories.Public)]
[Category(TestCategories.Auctions)]
[Category(TestCategories.Regression)]
[Parallelizable(ParallelScope.Self)]
public sealed class AuctionTests : UiTestBase
{
    [Test]
    public void Auctions_Page_Shows_Heading_And_Coming_Soon_Message()
    {
        var page = new AuctionsPage(Session);
        page.Open();
        Assert.Multiple(() =>
        {
            Assert.That(page.HeadingText, Is.EqualTo("Car auctions in Dubai"));
            Assert.That(page.IntroTextValue, Does.Contain("highest bidder"));
            Assert.That(page.ShowsComingSoon, Is.True);
            Assert.That(page.ComingSoonText, Does.Contain("coming soon"));
            Assert.That(page.HasAuctionGrid, Is.False);
        });
    }

    [Test]
    public void Auctions_Visible_Query_Shows_Grid_And_Filters()
    {
        var page = new AuctionsPage(Session);
        page.OpenWithAuctionsVisible();
        Assert.Multiple(() =>
        {
            Assert.That(page.ShowsComingSoon, Is.False);
            Assert.That(page.HasAuctionGrid, Is.True);
            Assert.That(page.HasFilters, Is.True);
        });
    }

    [TestCase("/auctions/sold")]
    [TestCase("/auctions/upcoming")]
    public async Task Auction_Status_Pages_Redirect_To_Auctions(string path)
    {
        using var http = NewHttpProbe();
        var response = await http.GetAsync(path, followRedirects: false);
        Assert.Multiple(() =>
        {
            Assert.That(response.IsRedirect, Is.True, response.ToString());
            Assert.That(response.Location?.OriginalString, Does.EndWith("/auctions"));
        });
    }

    [Test]
    public void Auction_Detail_Opens_When_Any_Active_Auction_Exists()
    {
        var page = new AuctionsPage(Session);
        page.OpenWithAuctionsVisible();
        var link = page.AuctionLinks().FirstOrDefault();
        if (link is null) Assert.Inconclusive("No active auctions on this environment.");
        var details = new AuctionDetailsPage(Session);
        details.OpenAuction(link!);
        Assert.Multiple(() =>
        {
            Assert.That(details.HeadingText, Is.Not.Empty);
            Assert.That(details.SectionTitleTexts(), Does.Contain("Key facts").Or.Contain("Overview"));
            Assert.That(UrlHelper.PathOf(Session.Driver.Url), Does.StartWith("/auctions/"));
        });
    }
}
