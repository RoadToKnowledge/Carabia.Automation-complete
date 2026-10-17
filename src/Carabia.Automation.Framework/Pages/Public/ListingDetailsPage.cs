using Carabia.Automation.Framework.Base;
using Carabia.Automation.Framework.Components.Common;
using Carabia.Automation.Framework.Components.Listing;
using Carabia.Automation.Framework.Utilities;
using OpenQA.Selenium;

namespace Carabia.Automation.Framework.Pages.Public;

/// <summary>
/// SellingsController.Selling -> Views/Sellings/Selling.cshtml, route /sellings/{id}_{modelName}.
/// </summary>
public sealed class ListingDetailsPage(BrowserSession session) : PageBase(session)
{
    private static readonly By VehicleHeader = By.CssSelector(".lps-vehicle-header h1");
    private static readonly By VehicleSubtitle = By.CssSelector(".lps-vehicle-header p");
    private static readonly By SectionTitle = By.CssSelector(".lps-section-title");
    private static readonly By DesktopPrice = By.CssSelector(".lps-price-desk .lps-price");
    private static readonly By MonthlyPayment = By.CssSelector(".lps-price-desk .lps-monthly-emi");
    private static readonly By SellerCard = By.CssSelector(".lps-seller-card .lps-seller-details h3");
    private static readonly By VinFields = By.CssSelector(".lps-vin-field");
    private static readonly By WishlistButton = By.CssSelector(".lps-btn-wishlist");
    private static readonly By AskQuestionLink = By.CssSelector("a.lps-btn-ask");
    private static readonly By CommentsSection = By.Id("ask-question");
    private static readonly By CommentTextarea = By.CssSelector("#ask-question textarea, #ask-question .comment-form textarea");
    private static readonly By CommentSubmitButton = By.CssSelector("#ask-question button[type='submit'], #ask-question .btn-submit");
    private static readonly By BackToListings = By.Id("backToListingBtn");
    private static readonly By RelatedCards = By.CssSelector(".listing-card");

    public GalleryComponent Gallery { get; } = new(session);
    public FeaturesComponent Features { get; } = new(session);
    public LocationMapComponent Map { get; } = new(session);
    public OfferComponent ContactOwner { get; } = new(session);
    public AdditionalServicesComponent Services { get; } = new(session);
    public FaqComponent Faq { get; } = new(session);
    public LoginPopupComponent LoginPopup { get; } = new(session);

    public void OpenListing(string pathOrUrl) =>
        Session.Open(UrlHelper.RouteOf(pathOrUrl));

    public void WaitUntilLoaded() => Session.Visible(SectionTitle, "the vehicle details heading (.lps-section-title)");

    public int? SellingId => UrlHelper.SellingIdFromUrl(Driver.Url);
    public string VehicleName => Driver.FindElements(VehicleHeader).FirstOrDefault()?.Text.Trim() ?? string.Empty;
    public string VehicleSubtitleText => Driver.FindElements(VehicleSubtitle).FirstOrDefault()?.Text.Trim() ?? string.Empty;
    public string SectionTitleText => Session.Visible(SectionTitle, "the vehicle details heading").Text.Trim();
    public string PriceText => Session.Visible(DesktopPrice, "the asking price").Text.Trim();
    public decimal Price => TextParser.Number(PriceText);

    /// <summary>The price is rendered twice (mobile card .lps-price-mobile and desktop sidebar .lps-price-desk).</summary>
    public bool AnyPriceVisible => Driver.FindElements(By.CssSelector(".lps-price")).Any(e => e.Displayed);
    public string MonthlyPaymentText => Driver.FindElements(MonthlyPayment).FirstOrDefault()?.Text.Trim() ?? string.Empty;
    public string SellerName => Driver.FindElements(SellerCard).FirstOrDefault()?.Text.Trim() ?? string.Empty;
    public int VinFieldCount => Driver.FindElements(VinFields).Count;
    public bool HasAskQuestion => Elements.Exists(AskQuestionLink) && Elements.Exists(CommentsSection);
    public bool HasCommentTextarea => Elements.Exists(CommentTextarea);
    public bool HasCommentSubmitButton => Elements.Exists(CommentSubmitButton);
    public bool HasBackToListings => Elements.Exists(BackToListings);
    public int RelatedListingCount => Driver.FindElements(RelatedCards).Count;

    public IWebElement? VisibleWishlistButton => Driver.FindElements(WishlistButton).FirstOrDefault(b => b.Displayed);

    /// <summary>For visitors the heart button carries "require-login" and opens the login popup.</summary>
    public void ClickWishlist()
    {
        var button = VisibleWishlistButton ?? throw new AutomationException("No visible favourites (heart) button on the listing.");
        Step("Listing: click favourites", button);
        Elements.Click(button);
    }

    public bool WishlistIsAdded =>
        VisibleWishlistButton is { } b && (b.GetDomAttribute("class") ?? string.Empty).Contains("bookmark-button_state_added");
}
