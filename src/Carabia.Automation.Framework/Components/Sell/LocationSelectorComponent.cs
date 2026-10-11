using Carabia.Automation.Framework.Base;
using Carabia.Automation.Framework.Models.TestData;
using OpenQA.Selenium;

namespace Carabia.Automation.Framework.Components.Sell;

/// <summary>
/// Step 2 location: Google Places autocomplete on #sellCarLocationInput (suggestions ".pac-item").
/// The selected place is written to hidden fields #sellCarLocationCity, #sellCarLocationCountry,
/// #sellCarLocationPlaceId, #sellCarLocationLatitude/Longitude.
/// </summary>
public sealed class LocationSelectorComponent(BrowserSession session) : ComponentBase(session)
{
    private const string Input = "sellCarLocationInput";
    private static readonly By Suggestions = By.CssSelector(".pac-item");

    private SellWizardComponent Wizard { get; } = new(session);

    public string City => Wizard.Value("sellCarLocationCity");
    public string CountryCode => Wizard.Value("sellCarLocationCountry");
    public string PlaceId => Wizard.Value("sellCarLocationPlaceId");
    public string Latitude => Wizard.Value("sellCarLocationLatitude");
    public string Longitude => Wizard.Value("sellCarLocationLongitude");

    public void TypeSearch(string text) => Wizard.Fill(Input, text, tabOut: false);

    public IReadOnlyList<string> VisibleSuggestions() =>
        Driver.FindElements(Suggestions).Where(e => e.Displayed).Select(e => e.Text.Trim()).ToList();

    public void Select(LocationTestData location)
    {
        TypeSearch(location.SearchText);
        var suggestion = Session.WaitFor($"a Google Places suggestion containing '{location.SuggestionContains}' (check the Maps API key if none appears)", d =>
            d.FindElements(Suggestions).FirstOrDefault(e => e.Displayed && e.Text.Contains(location.SuggestionContains, StringComparison.OrdinalIgnoreCase)),
            Session.LongTimeout);
        Wizard.Click(suggestion!, $"Sell: choose location suggestion '{suggestion!.Text.Trim()}'");
        Session.WaitFor($"the location to resolve to country {location.ExpectedCountryCode}", _ =>
            CountryCode == location.ExpectedCountryCode && PlaceId.Length > 0, Session.LongTimeout);
    }
}
