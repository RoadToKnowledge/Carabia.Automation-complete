using Carabia.Automation.Framework.Base;
using Carabia.Automation.Framework.Utilities;
using OpenQA.Selenium;

namespace Carabia.Automation.Framework.Components.Search;

/// <summary>
/// A listing card (Views/Shared/Components/Cards/Selling.cshtml, ".listing-card").
/// Selectors proven by carabia-ui-tests ListingsPresentationTests / ListingsFilterPage.
/// </summary>
public sealed class CarCardComponent(BrowserSession session, IWebElement root) : ComponentBase(session)
{
    public static readonly By CardSelector = By.CssSelector(".listing-card");
    private static readonly By TitleLink = By.CssSelector(".listing-card__title-link");
    private static readonly By Meta = By.CssSelector(".listing-card__meta");
    private static readonly By MetaParts = By.CssSelector(".listing-card__meta > span");
    private static readonly By Price = By.CssSelector(".listing-card__price");
    private static readonly By Image = By.CssSelector(".listing-card-image__img");

    public IWebElement Root { get; } = root;

    public IWebElement TitleElement => Root.FindElement(TitleLink);
    public string Title => TitleElement.Text.Trim();
    public string Href => TitleElement.GetDomProperty("href") ?? string.Empty;
    public string MetaText => Root.FindElements(Meta).FirstOrDefault()?.Text.Trim() ?? string.Empty;
    public string PriceText => Root.FindElements(Price).FirstOrDefault()?.Text.Trim() ?? string.Empty;
    public IWebElement? ImageElement => Root.FindElements(Image).FirstOrDefault();

    public bool HasTitle => Root.FindElements(TitleLink).Count > 0;
    public bool HasMeta => Root.FindElements(Meta).Count > 0;
    public bool HasPrice => Root.FindElements(Price).Count > 0;
    public bool HasImage => Root.FindElements(Image).Count > 0;

    /// <summary>Year and mileage are the first and last "meta" spans; price is the price element.</summary>
    public CarCard Read()
    {
        var meta = Root.FindElements(MetaParts);
        return new CarCard(Title, Href,
            meta.Count > 0 ? TextParser.Number(meta.First().Text) : 0,
            meta.Count > 0 ? TextParser.Number(meta.Last().Text) : 0,
            TextParser.Number(PriceText));
    }

    public void Open()
    {
        var link = TitleElement;
        Step($"Open listing card: {link.Text.Trim()}", link);
        Elements.Click(link);
    }
}

public sealed record CarCard(string Title, string Url, decimal Year, decimal Mileage, decimal Price)
{
    public bool MatchesVehicle(string expected) =>
        Title.Equals(expected, StringComparison.OrdinalIgnoreCase) || Title.StartsWith(expected + " ", StringComparison.OrdinalIgnoreCase);
}
