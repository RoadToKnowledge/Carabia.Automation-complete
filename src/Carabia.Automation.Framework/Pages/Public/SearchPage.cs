using Carabia.Automation.Framework.Base;
using Carabia.Automation.Framework.Components.Common;
using Carabia.Automation.Framework.Components.Search;
using OpenQA.Selenium;

namespace Carabia.Automation.Framework.Pages.Public;

/// <summary>
/// SellingsController.Index -> Views/Sellings/Index.cshtml: /sellings, /sellings/page-N, /sellings/sold and
/// collection routes /sellings/{quick filter|body-*|model-*|location-*|under-*}.
/// Heading .fd-banner__title, results .crb-listing-results (.crb-results-count, .listing-card).
/// </summary>
public class SearchPage(BrowserSession session) : PageBase(session)
{
    private static readonly By Heading = By.CssSelector(".fd-banner__title");
    private static readonly By Results = By.CssSelector(".crb-listing-results");
    private static readonly By ResultsCount = By.CssSelector(".crb-listing-results .crb-results-count");
    private static readonly By CardSelector = By.CssSelector(".crb-listing-results .listing-card");
    private static readonly By EmptyResultsPanel = By.CssSelector(".crb-listing-results .nrf-card-container");
    private static readonly By EmptyResultsTitle = By.CssSelector(".nrf-title-heading");

    protected override string Path => "/sellings";

    public FilterComponent Filters { get; } = new(session);
    public PaginationComponent Pagination { get; } = new(session);
    public SitemapLinksComponent SitemapLinks { get; } = new(session);
    public FaqComponent Faq { get; } = new(session);
    public FooterComponent Footer { get; } = new(session);

    public void OpenCollection(string slug) => Session.Open($"/sellings/{slug.Trim('/')}");

    public string HeadingText => Session.Visible(Heading, "the listings page heading (.fd-banner__title)").Text.Trim();

    public bool HasResultsArea => Elements.Exists(Results);

    public int ResultCount => (int)Utilities.TextParser.Number(Session.Visible(ResultsCount, "the result count").Text);

    /// <summary>Waits until at least one card is rendered (the first page renders server-side).</summary>
    public IReadOnlyList<CarCardComponent> WaitForCards()
    {
        var cards = Session.WaitFor("at least one car listing (check that the environment has active listings)", d =>
        {
            var found = d.FindElements(CardSelector);
            return found.Count > 0 ? found : null;
        });
        return cards!.Select(e => new CarCardComponent(Session, e)).ToList();
    }

    public IReadOnlyList<CarCardComponent> Cards() => Driver.FindElements(CardSelector).Select(e => new CarCardComponent(Session, e)).ToList();

    public bool ShowsEmptyResultsPanel => Elements.IsDisplayed(EmptyResultsPanel);
    public string EmptyResultsTitleText => Driver.FindElements(EmptyResultsTitle).FirstOrDefault()?.Text.Trim() ?? string.Empty;
}

/// <summary>Collection pages are the same view with a slug; kept as a named page object for readability in tests.</summary>
public sealed class CollectionPage(BrowserSession session) : SearchPage(session);
