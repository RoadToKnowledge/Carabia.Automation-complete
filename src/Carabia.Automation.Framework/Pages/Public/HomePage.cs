using Carabia.Automation.Framework.Base;
using Carabia.Automation.Framework.Components.Common;
using Carabia.Automation.Framework.Components.Search;
using Carabia.Automation.Framework.Utilities;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace Carabia.Automation.Framework.Pages.Public;

/// <summary>
/// HomeController.Index -> Views/Home/Index.cshtml.
/// Hero search: #crbHeroMakeSelect, #crbHeroModelSelect (models loaded from /System/GetCarModels),
/// #crbHeroSearchButton (count from /System/GetSellingSearchCount). Search navigates to
/// /sellings/model-{slug}, /brand/{slug} or /sellings. Quick filters: .crb-filter-badge.
/// </summary>
public sealed class HomePage(BrowserSession session) : PageBase(session)
{
    private static readonly By HeroMakeSelect = By.Id("crbHeroMakeSelect");
    private static readonly By HeroModelSelect = By.Id("crbHeroModelSelect");
    private static readonly By HeroSearchButton = By.Id("crbHeroSearchButton");
    private static readonly By HeroSearchCount = By.CssSelector("#crbHeroSearchButton .crb-s2-search-count");
    private static readonly By HeroSearchNoun = By.CssSelector("#crbHeroSearchButton .crb-s2-search-noun");
    private static readonly By SellForFreeTab = By.CssSelector(".crb-s2-tabs-row .crb-s2-tab-item[type='button']");
    private static readonly By QuickFilterBadges = By.CssSelector(".crb-filter-badge");
    private static readonly By FeaturedCards = By.CssSelector(".listing-card");
    private static readonly By SeeAllButtons = By.CssSelector("a.crb-news-see-all-btn");

    protected override string Path => "/";

    public HeaderComponent Header { get; } = new(session);
    public MobileNavigationComponent MobileNavigation { get; } = new(session);
    public FooterComponent Footer { get; } = new(session);
    public SitemapLinksComponent SitemapLinks { get; } = new(session);
    public FaqComponent Faq { get; } = new(session);
    public LoginPopupComponent LoginPopup { get; } = new(session);

    public bool HasHeroSearch => Elements.IsDisplayed(HeroMakeSelect) && Elements.IsDisplayed(HeroModelSelect) && Elements.IsDisplayed(HeroSearchButton);

    public int HeroSearchCountValue => (int)TextParser.Number(Driver.FindElement(HeroSearchCount).Text);
    public string HeroSearchNounText => Driver.FindElement(HeroSearchNoun).Text.Trim();
    public int InitialSearchCount => int.TryParse(Driver.FindElement(HeroSearchButton).GetDomAttribute("data-initial-count"), out var c) ? c : -1;

    public SelectElement MakeSelect => new(Driver.FindElement(HeroMakeSelect));
    public SelectElement ModelSelect => new(Driver.FindElement(HeroModelSelect));

    public IReadOnlyList<(string Text, string Slug)> MakeOptions() =>
        MakeSelect.Options.Where(o => !string.IsNullOrEmpty(o.GetDomAttribute("value")))
            .Select(o => (o.Text.Trim(), o.GetDomAttribute("data-slug") ?? string.Empty)).ToList();

    /// <summary>Selects a make and waits for its models and the refreshed car count.</summary>
    public void ChooseMake(string make)
    {
        var option = MakeSelect.Options.FirstOrDefault(o => o.Text.Trim().Equals(make, StringComparison.OrdinalIgnoreCase))
                     ?? throw new AutomationException($"The hero make list has no '{make}'.");
        Step($"Home search: choose make {make}", Driver.FindElement(HeroMakeSelect));
        MakeSelect.SelectByValue(option.GetDomAttribute("value")!);
        WaitHelper.ForSelectOptions(Driver, HeroModelSelect, $"models for {make} to load in the hero search", Session.LongTimeout);
    }

    public string ChooseFirstModel()
    {
        var option = ModelSelect.Options.First(o => !string.IsNullOrEmpty(o.GetDomAttribute("value")));
        Step($"Home search: choose model {option.Text.Trim()}", Driver.FindElement(HeroModelSelect));
        ModelSelect.SelectByValue(option.GetDomAttribute("value")!);
        return option.Text.Trim();
    }

    public string SelectedModelSlug => ModelSelect.SelectedOption.GetDomAttribute("data-slug") ?? string.Empty;
    public string SelectedMakeSlug => MakeSelect.SelectedOption.GetDomAttribute("data-slug") ?? string.Empty;

    public void WaitForSearchCountChange(int previous) =>
        WaitHelper.TryUntil(Driver, _ => HeroSearchCountValue != previous, Session.DefaultTimeout);

    public void ClickSearch()
    {
        var button = Elements.Clickable(HeroSearchButton, "the hero Search button");
        Step("Home search: click Search", button);
        Elements.Click(button);
    }

    public void ClickSellForFreeTab()
    {
        var tab = Elements.Clickable(SellForFreeTab, "the 'Sell for free' tab");
        Step("Home: click 'Sell for free'", tab);
        Elements.Click(tab);
    }

    public IReadOnlyList<(string Key, string Text, string Href)> QuickFilters() =>
        Driver.FindElements(QuickFilterBadges)
            .Select(b => (b.GetDomAttribute("data-filter-key") ?? string.Empty,
                b.FindElements(By.CssSelector(".crb-badge-text")).FirstOrDefault()?.Text.Trim() ?? b.Text.Trim(),
                b.GetDomProperty("href") ?? string.Empty))
            .ToList();

    public void ClickQuickFilter(string key)
    {
        var badge = Driver.FindElements(QuickFilterBadges).FirstOrDefault(b => b.GetDomAttribute("data-filter-key") == key)
                    ?? throw new AutomationException($"Quick filter '{key}' is not on the home page.");
        Step($"Home: click quick filter {key}", badge);
        Elements.Click(badge);
    }

    public IReadOnlyList<CarCardComponent> FeaturedListings() =>
        Driver.FindElements(FeaturedCards).Where(c => c.Displayed).Select(c => new CarCardComponent(Session, c)).ToList();

    public CarCardComponent? FeaturedListingByPath(string path) =>
        Driver.FindElements(FeaturedCards)
            .Select(c => new CarCardComponent(Session, c))
            .FirstOrDefault(c => c.HasTitle && c.TitleElement.Displayed && UrlHelper.SamePath(c.Href, path));

    public int SeeAllButtonCount => Driver.FindElements(SeeAllButtons).Count;
}
