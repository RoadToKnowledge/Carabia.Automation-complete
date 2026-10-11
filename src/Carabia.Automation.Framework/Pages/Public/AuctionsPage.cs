using Carabia.Automation.Framework.Base;
using OpenQA.Selenium;

namespace Carabia.Automation.Framework.Pages.Public;

/// <summary>
/// AuctionsController.Index -> Views/Auctions/Index.cshtml.
/// /auctions shows "Auctions are coming soon to Carabia." (.illustration__caption) unless the
/// query string contains auctions_visible, which renders the grid (.product-grid__list.items_container).
/// /auctions/sold and /auctions/upcoming currently redirect to /auctions (controller returns Redirect).
/// </summary>
public sealed class AuctionsPage(BrowserSession session) : PageBase(session)
{
    private static readonly By Heading = By.CssSelector(".section_type_intro .section__title h1");
    private static readonly By IntroText = By.CssSelector(".section_type_intro .section__text");
    private static readonly By ComingSoonCaption = By.CssSelector(".illustration__caption");
    private static readonly By AuctionGrid = By.CssSelector(".product-grid__list.items_container");
    private static readonly By AuctionCardLinks = By.CssSelector(".product-grid__list.items_container a[href*='/auctions/']");
    private static readonly By Filters = By.CssSelector(".product-grid__filters");

    protected override string Path => "/auctions";

    public void OpenWithAuctionsVisible() => Session.Open("/auctions?auctions_visible=1");

    public string HeadingText => Session.Visible(Heading, "the auctions heading").Text.Trim();
    public string IntroTextValue => Driver.FindElements(IntroText).FirstOrDefault()?.Text.Trim() ?? string.Empty;
    public bool ShowsComingSoon => Elements.IsDisplayed(ComingSoonCaption);
    public string ComingSoonText => Driver.FindElements(ComingSoonCaption).FirstOrDefault()?.Text.Trim() ?? string.Empty;
    public bool HasAuctionGrid => Elements.Exists(AuctionGrid);
    public bool HasFilters => Elements.Exists(Filters);

    public IReadOnlyList<string> AuctionLinks() =>
        Driver.FindElements(AuctionCardLinks).Select(a => a.GetDomProperty("href") ?? string.Empty).Where(h => h.Length > 0).Distinct().ToList();
}

/// <summary>AuctionsController.Auction -> Views/Auctions/Auction.cshtml, route /auctions/{id}_{modelName}.</summary>
public sealed class AuctionDetailsPage(BrowserSession session) : PageBase(session)
{
    private static readonly By Heading = By.CssSelector("h1.detail__heading");
    private static readonly By SectionTitles = By.CssSelector(".detail__title, .panel__title");

    public void OpenAuction(string url) => Session.Open(Utilities.UrlHelper.RouteOf(url));

    public string HeadingText => Session.Visible(Heading, "the auction heading (h1.detail__heading)").Text.Trim();

    public IReadOnlyList<string> SectionTitleTexts() =>
        Driver.FindElements(SectionTitles).Select(e => e.Text.Trim()).Where(t => t.Length > 0).ToList();
}
