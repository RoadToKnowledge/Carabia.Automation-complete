using Carabia.Automation.Framework.Base;
using Carabia.Automation.Framework.Components.Common;
using OpenQA.Selenium;

namespace Carabia.Automation.Framework.Pages.Public;

/// <summary>HomeController.Brand -> Views/Home/Brand.cshtml: /brand/{brand}[/location-{location}][/page-N].</summary>
public sealed class BrandPage(BrowserSession session) : PageBase(session)
{
    private static readonly By Heading = By.CssSelector(".fd-banner__title");
    private static readonly By Cards = By.CssSelector(".listing-card");

    public FaqComponent Faq { get; } = new(session);
    public SitemapLinksComponent SitemapLinks { get; } = new(session);

    public void OpenBrand(string brandSlug, string? locationSlug = null) =>
        Session.Open(locationSlug is null ? $"/brand/{brandSlug}" : $"/brand/{brandSlug}/location-{locationSlug}");

    public string HeadingText => Session.Visible(Heading, "the brand page heading").Text.Trim();
    public int CardCount => Driver.FindElements(Cards).Count(c => c.Displayed);
}
