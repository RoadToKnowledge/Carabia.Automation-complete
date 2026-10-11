using System.Globalization;
using Carabia.Automation.Framework.Base;
using Carabia.Automation.Framework.Utilities;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace Carabia.Automation.Framework.Components.Search;

/// <summary>
/// Desktop filter sidebar of /sellings (Views/Sellings/Partial/_FiltersPartial.cshtml).
/// Every change reloads the results via AJAX (SellingsController.IndexListing), which replaces
/// ".crb-listing-results .crb-results-count" - the old element going stale is the "results updated" signal.
/// Ported from carabia-ui-tests ListingsFilterPage (verified against dev.carabiacars.com).
/// </summary>
public sealed class FilterComponent(BrowserSession session) : ComponentBase(session)
{
    public const string Make = "filters_manufacturer";
    public const string Model = "filters_model";
    public static readonly string[] RangeDropdowns =
        ["filters_price_from", "filters_price_to", "filters_year_from", "filters_year_to", "filters_mileage_from", "filters_mileage_to"];

    private const string Results = ".crb-listing-results";
    private static readonly By Header = By.CssSelector(".crb-filter-header");
    private static readonly By Sidebar = By.Id("crbSidebar");
    private static readonly By ResultCount = By.CssSelector(Results + " .crb-results-count");
    private static readonly By ClearAll = By.Id("crbClearAllBtn");
    private static readonly By CheckedSort = By.CssSelector("input[name='crb_sort']:checked");

    public bool IsDisplayed => Elements.IsDisplayed(Header) && Elements.Exists(Sidebar);

    public void WaitUntilReady() => Session.Visible(Header, "the listings filter sidebar");

    public int ResultCountValue => (int)TextParser.Number(Driver.FindElement(ResultCount).Text);

    public SelectElement Select(string name) => new(Driver.FindElement(By.Name(name)));

    public string SelectedValue(string name) =>
        Select(name).AllSelectedOptions.FirstOrDefault()?.GetDomAttribute("value") ?? "(none)";

    public string CheckedSortValue => Driver.FindElement(CheckedSort).GetDomAttribute("value") ?? string.Empty;

    public void Choose(string name, string value)
    {
        var select = Select(name);
        ExpandSectionOf(select.WrappedElement);
        var option = select.Options.FirstOrDefault(o => o.GetDomAttribute("value") == value)
                     ?? throw new AutomationException($"Filter '{name}' does not offer value '{value}'. Available: {string.Join(", ", select.Options.Select(o => o.Text))}.");
        if (select.SelectedOption.GetDomAttribute("value") == value)
            throw new AutomationException($"Test setup: '{name}' is already '{option.Text}'; choose a different value to exercise filtering.");
        RefreshAfter($"Set {Label(name)} to {option.Text}", select.WrappedElement, () => select.SelectByValue(value));
    }

    public void ChooseMake(string make)
    {
        ExpandSectionOf(Select(Make).WrappedElement);
        var option = Select(Make).Options.FirstOrDefault(o => o.Text.Trim().Equals(make, StringComparison.OrdinalIgnoreCase))
                     ?? throw new AutomationException($"Test data needs an available '{make}' make in the filter dropdown.");
        Choose(Make, option.GetDomAttribute("value")!);
    }

    /// <summary>Picks a model of <paramref name="make"/> that appears on the current results page (longest name first, e.g. "A6 Allroad" before "A6").</summary>
    public string ChooseModelPresentInResults(string make)
    {
        ExpandSectionOf(Select(Model).WrappedElement);
        Session.WaitFor($"models for {make} to load (check /System/GetCarModels if it stays on Loading)", _ =>
            !Select(Model).Options.Any(o => o.Text.Contains("Loading", StringComparison.OrdinalIgnoreCase)));
        if (!Select(Model).WrappedElement.Enabled || Select(Model).Options.Count <= 1)
            throw new AutomationException($"After selecting {make}, the model dropdown is disabled or empty. " +
                                          "Check the /System/GetCarModels response; model filtering cannot continue.");
        var cards = MatchingCards();
        var option = Select(Model).Options.Where(o => !string.IsNullOrEmpty(o.GetDomAttribute("value")))
                         .OrderByDescending(o => o.Text.Length)
                         .FirstOrDefault(o => cards.Any(c => c.MatchesVehicle(make + " " + o.Text.Trim())))
                     ?? throw new AutomationException($"No {make} model option matches the cars on the page.");
        var model = option.Text.Trim();
        Choose(Model, option.GetDomAttribute("value")!);
        return model;
    }

    public void Sort(string value)
    {
        var radio = Driver.FindElement(By.CssSelector($"input[name='crb_sort'][value='{value}']"));
        ExpandSectionOf(radio);
        var label = radio.FindElement(By.XPath("ancestor::label[1]"));
        RefreshAfter($"Sort: {label.Text.Trim()}", label, label.Click);
    }

    public void ClearAllFilters()
    {
        var button = Driver.FindElement(ClearAll);
        RefreshAfter("Clear all filters", button, button.Click);
    }

    /// <summary>Selects a bounded range around a car that is on the page, so an empty result can never pass.</summary>
    public (decimal From, decimal To) ChooseRangeAroundExistingCar(string field, Func<CarCard, decimal> read)
    {
        var fromName = $"filters_{field}_from";
        var toName = $"filters_{field}_to";
        var lower = NumericOptions(fromName);
        var upper = NumericOptions(toName);
        foreach (var car in MatchingCards())
        {
            var value = read(car);
            var from = lower.Where(o => o.Value < value && !o.Default).OrderByDescending(o => o.Value).FirstOrDefault();
            var to = upper.Where(o => o.Value >= value && !o.Default && !o.Unbounded).OrderBy(o => o.Value).FirstOrDefault();
            if (from is null || to is null || to.Value <= from.Value) continue;
            Choose(fromName, from.Raw);
            Choose(toName, to.Raw);
            return (from.Value, to.Value);
        }

        throw new AutomationException($"Test data needs a car inside the bounded {field} ranges; none was found on the current results page.");
    }

    /// <summary>
    /// Cards that match the filters. Recommendations after the "That's everything" panel (.nrf-card-container)
    /// are excluded, as they intentionally do not match.
    /// </summary>
    public List<CarCard> MatchingCards()
    {
        var root = Driver.FindElement(By.CssSelector(Results));
        var cards = new List<CarCard>();
        foreach (var child in root.FindElements(By.XPath("./*")))
        {
            if ((child.GetDomAttribute("class") ?? string.Empty).Split(' ').Contains("nrf-card-container")) break;
            cards.AddRange(child.FindElements(CarCardComponent.CardSelector).Select(e => new CarCardComponent(Session, e).Read()));
        }
        return cards;
    }

    public bool ShowsEndOfResultsPanel => Elements.Exists(By.CssSelector(Results + " .nrf-card-container"));

    private void ExpandSectionOf(IWebElement control)
    {
        var section = control.FindElement(By.XPath("ancestor::section[contains(@class,'crb-filter-section')][1]"));
        if (!(section.GetDomAttribute("class") ?? string.Empty).Contains("crb-section--collapsed")) return;
        var title = section.FindElement(By.CssSelector(".crb-section-title"));
        Step($"Open {title.Text.Trim()} filter", title);
        title.Click();
        Session.WaitFor("the filter section to expand", _ => !(section.GetDomAttribute("class") ?? string.Empty).Contains("crb-section--collapsed"));
    }

    private void RefreshAfter(string action, IWebElement control, Action change)
    {
        var previous = Driver.FindElement(ResultCount);
        Step(action, control);
        change();
        WaitHelper.ForStale(Driver, previous, $"listing results to refresh after '{action}'", Session.LongTimeout);
        Session.Visible(ResultCount, "the updated result count");
        Session.Log($"{action}: {ResultCountValue} matching results");
    }

    private List<NumericOption> NumericOptions(string name) => Select(name).Options
        .Where(o => decimal.TryParse(o.GetDomAttribute("value"), NumberStyles.Number, CultureInfo.InvariantCulture, out _))
        .Select(o => new NumericOption(o.GetDomAttribute("value")!,
            decimal.Parse(o.GetDomAttribute("value")!, CultureInfo.InvariantCulture),
            o.GetDomAttribute("data-filter-default") == "true",
            o.GetDomAttribute("data-price-unbounded") == "true" || o.GetDomAttribute("data-range-unbounded") == "true"))
        .ToList();

    private static string Label(string name) => name.Replace("filters_", "").Replace('_', ' ');

    private sealed record NumericOption(string Raw, decimal Value, bool Default, bool Unbounded);
}
