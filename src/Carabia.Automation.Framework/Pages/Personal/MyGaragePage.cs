using Carabia.Automation.Framework.Base;
using Carabia.Automation.Framework.Utilities;
using OpenQA.Selenium;

namespace Carabia.Automation.Framework.Pages.Personal;

/// <summary>
/// "My Garage" = PersonalController.Buying / Selling / Favorites -> Views/Personal/Garage.cshtml.
/// Tabs .tabs__label (active: tabs__label_state_active): Buying (/personal/buying), Selling (/personal/selling),
/// Favourites (/personal/favorites, read from the carabia_favorites cookie).
/// Status groups .product-status-group__title h2 e.g. "Submissions (1)", "Live (2)".
/// Cards .product-status-card (Views/Personal/Partial/_GarageSellingPartial.cshtml).
/// </summary>
public sealed class MyGaragePage(BrowserSession session) : PersonalPageBase(session)
{
    public enum GarageTab { Buying, Selling, Favourites }

    private static readonly By Tabs = By.CssSelector(".tabs__nav .tabs__label");
    private static readonly By ActiveTab = By.CssSelector(".tabs__nav .tabs__label_state_active");
    private static readonly By GroupTitles = By.CssSelector(".product-status-group__title h2");
    private static readonly By Cards = By.CssSelector(".product-status-card");
    private static readonly By CardTitle = By.CssSelector(".product-status-card__title .title");
    private static readonly By ViewListingLink = By.CssSelector(".product-status-card__control a.button[href*='/sellings/']");
    private static readonly By ManageOfferButton = By.CssSelector(".manage_offer_button");
    private static readonly By EditPriceLink = By.CssSelector("a.js-modal[href='#modalEditPrice']");
    private static readonly By RemoveListingLink = By.CssSelector("a.js-modal[href='#modalRemoveListing']");

    private GarageTab _tab = GarageTab.Buying;

    protected override string Path => _tab switch
    {
        GarageTab.Selling => "/personal/selling",
        GarageTab.Favourites => "/personal/favorites",
        _ => "/personal/buying"
    };

    public void OpenTab(GarageTab tab)
    {
        _tab = tab;
        Open();
    }

    public IReadOnlyList<string> TabTexts() => Driver.FindElements(Tabs).Select(t => t.Text.Trim()).ToList();
    public string ActiveTabText => Driver.FindElements(ActiveTab).FirstOrDefault()?.Text.Trim() ?? string.Empty;

    public void ClickTab(string text)
    {
        var tab = Driver.FindElements(Tabs).FirstOrDefault(t => t.Text.Trim().Equals(text, StringComparison.OrdinalIgnoreCase))
                  ?? throw new AutomationException($"My Garage has no '{text}' tab.");
        Step($"My Garage: click tab {text}", tab);
        Elements.Click(tab);
        WaitHelper.ForDocumentReady(Driver, Session.LongTimeout);
    }

    /// <summary>Status groups with their counts, e.g. ("Submissions", 1).</summary>
    public IReadOnlyList<(string Name, int Count)> StatusGroups() =>
        Driver.FindElements(GroupTitles)
            .Select(h => h.Text.Trim())
            .Where(t => t.Length > 0)
            .Select(t => (Name: t.Split('(')[0].Trim(), Count: TextParser.TryNumber(t, out var n) ? (int)n : 0))
            .ToList();

    public IReadOnlyList<IWebElement> CardElements() => Driver.FindElements(Cards);

    public IReadOnlyList<string> CardTitles() =>
        Driver.FindElements(CardTitle).Select(t => t.Text.Trim()).Where(t => t.Length > 0).ToList();

    public IReadOnlyList<string> ViewListingHrefs() =>
        Driver.FindElements(ViewListingLink).Select(a => a.GetDomProperty("href") ?? string.Empty).Where(h => h.Length > 0).ToList();

    /// <summary>The card whose "View Listing" link points at the given selling id.</summary>
    public IWebElement? CardForSelling(int sellingId) =>
        Driver.FindElements(Cards).FirstOrDefault(card =>
            card.FindElements(ViewListingLink).Any(a => UrlHelper.SellingIdFromUrl(a.GetDomProperty("href") ?? "") == sellingId));

    public bool CardHasManageOffer(IWebElement card) => card.FindElements(ManageOfferButton).Count > 0;
    public bool CardHasEditPrice(IWebElement card) => card.FindElements(EditPriceLink).Count > 0;
    public bool CardHasRemoveListing(IWebElement card) => card.FindElements(RemoveListingLink).Count > 0;

    public void OpenManageOffer(IWebElement card)
    {
        var button = card.FindElement(ManageOfferButton);
        Step("My Garage: Manage offer", button);
        Elements.Click(button);
    }

    public void OpenEditPrice(IWebElement card)
    {
        var link = card.FindElement(EditPriceLink);
        Step("My Garage: Edit price", link);
        Elements.Click(link);
    }

    public void OpenRemoveListing(IWebElement card)
    {
        var link = card.FindElement(RemoveListingLink);
        Step("My Garage: Remove listing (opens dialog only)", link);
        Elements.Click(link);
    }
}
