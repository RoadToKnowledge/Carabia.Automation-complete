using Carabia.Automation.Framework.Components.Search;
using Carabia.Automation.Framework.Pages.Public;
using Carabia.Automation.Tests.Base;

namespace Carabia.Automation.Tests.Search;

/// <summary>
/// /sellings sidebar filters (make, model, price/year/mileage ranges, combined, clear all).
/// Ported from carabia-ui-tests ListingsFilterTests. Ranges are derived from a car on the page,
/// so an empty result can never pass.
/// </summary>
[TestFixture]
[Category(TestCategories.Public)]
[Category(TestCategories.Search)]
[Category(TestCategories.Regression)]
[Parallelizable(ParallelScope.Self)]
public sealed class FilterTests : UiTestBase
{
    private FilterComponent OpenFilters()
    {
        var page = new SearchPage(Session);
        page.Open();
        page.Filters.WaitUntilReady();
        Assert.That(page.Filters.ResultCountValue, Is.GreaterThan(0), "Filter tests need active listings on /sellings.");
        return page.Filters;
    }

    private static void AssertAllMatch(IReadOnlyCollection<CarCard> cards, string expected)
    {
        Assert.That(cards, Is.Not.Empty, $"No cards matched '{expected}'.");
        var wrong = cards.Where(c => !c.MatchesVehicle(expected)).Select(c => $"'{c.Title}' ({c.Url})").ToList();
        Assert.That(wrong, Is.Empty, $"Selected '{expected}' but results contain:\n" + string.Join("\n", wrong));
    }

    private static void AssertAllInRange(IReadOnlyCollection<CarCard> cards, string field, (decimal From, decimal To) range, Func<CarCard, decimal> read)
    {
        Assert.That(cards, Is.Not.Empty);
        var outside = cards.Where(c => read(c) < range.From || read(c) > range.To)
            .Select(c => $"'{c.Title}' shows {read(c):N0} ({c.Url})").ToList();
        Assert.That(outside, Is.Empty, $"{field} {range.From:N0}-{range.To:N0}:\n" + string.Join("\n", outside));
    }

    [TestCaseSource(typeof(TestData), nameof(TestData.MakesWithListings))]
    public void Selecting_A_Make_Shows_Only_That_Make(string make)
    {
        var filters = OpenFilters();
        filters.ChooseMake(make);
        AssertAllMatch(filters.MatchingCards(), make);
    }

    [TestCaseSource(typeof(TestData), nameof(TestData.MakesWithListings))]
    public void Selecting_A_Model_Shows_Only_That_Make_And_Model(string make)
    {
        var filters = OpenFilters();
        filters.ChooseMake(make);
        var model = filters.ChooseModelPresentInResults(make);
        AssertAllMatch(filters.MatchingCards(), $"{make} {model}");
    }

    [Test]
    public void Changing_Make_Resets_The_Previous_Model()
    {
        var makes = TestData.Search.MakesWithListings;
        if (makes.Count < 2) Assert.Ignore("Needs two makes with listings in testdata/Search/SearchCases.json.");
        var filters = OpenFilters();
        filters.ChooseMake(makes[0]);
        filters.ChooseModelPresentInResults(makes[0]);
        filters.ChooseMake(makes[1]);
        Assert.That(filters.SelectedValue(FilterComponent.Model), Is.EqualTo(""), $"Changing to {makes[1]} should reset the model to Any.");
        AssertAllMatch(filters.MatchingCards(), makes[1]);
    }

    [TestCase("price")]
    [TestCase("year")]
    [TestCase("mileage")]
    public void Numeric_Range_Filter_Keeps_Every_Car_Inside_The_Range(string field)
    {
        var filters = OpenFilters();
        Func<CarCard, decimal> read = field switch { "price" => c => c.Price, "year" => c => c.Year, _ => c => c.Mileage };
        var range = filters.ChooseRangeAroundExistingCar(field, read);
        AssertAllInRange(filters.MatchingCards(), field, range, read);
    }

    [Test]
    public void Make_And_Price_Filters_Work_Together()
    {
        var make = TestData.Search.MakesWithListings.First();
        var filters = OpenFilters();
        filters.ChooseMake(make);
        var range = filters.ChooseRangeAroundExistingCar("price", c => c.Price);
        var cards = filters.MatchingCards();
        AssertAllMatch(cards, make);
        AssertAllInRange(cards, "price", range, c => c.Price);
    }

    [Test]
    public void Clear_All_Restores_Default_Filters_Sorting_And_Result_Count()
    {
        var filters = OpenFilters();
        var initialCount = filters.ResultCountValue;
        var names = new[] { FilterComponent.Make, FilterComponent.Model }.Concat(FilterComponent.RangeDropdowns).ToArray();
        var defaults = names.ToDictionary(n => n, filters.SelectedValue);

        filters.ChooseMake(TestData.Search.MakesWithListings.First());
        filters.Sort("cheaper");
        filters.ChooseRangeAroundExistingCar("price", c => c.Price);
        filters.ClearAllFilters();

        var resetErrors = names.Where(n => filters.SelectedValue(n) != defaults[n])
            .Select(n => $"{n}: expected '{defaults[n]}', got '{filters.SelectedValue(n)}'").ToList();
        Assert.Multiple(() =>
        {
            Assert.That(filters.Select(FilterComponent.Model).WrappedElement.Enabled, Is.False, "Model must be disabled again until a make is chosen.");
            Assert.That(filters.CheckedSortValue, Is.EqualTo("newest"), "Clear all should restore Relevance sorting.");
            Assert.That(filters.ResultCountValue, Is.EqualTo(initialCount), "Clear all should restore the unfiltered result count.");
            Assert.That(resetErrors, Is.Empty, "Dropdowns not reset:\n" + string.Join("\n", resetErrors));
        });
    }

    [Test]
    public void Impossible_Filter_Combination_Shows_End_Of_Results_Panel()
    {
        var filters = OpenFilters();
        // Highest "from" price together with the lowest "to" price cannot match any car.
        var priceFrom = filters.Select("filters_price_from").Options.Last(o => !string.IsNullOrEmpty(o.GetDomAttribute("value")));
        var priceTo = filters.Select("filters_price_to").Options.First(o => !string.IsNullOrEmpty(o.GetDomAttribute("value"))
                                                                          && o.GetDomAttribute("data-filter-default") != "true");
        filters.Choose("filters_price_from", priceFrom.GetDomAttribute("value")!);
        filters.Choose("filters_price_to", priceTo.GetDomAttribute("value")!);
        Assert.Multiple(() =>
        {
            Assert.That(filters.ResultCountValue, Is.EqualTo(0), "An impossible price range should report 0 results.");
            Assert.That(filters.ShowsEndOfResultsPanel, Is.True, "The 'That's everything' panel (.nrf-card-container) should be shown.");
            Assert.That(filters.MatchingCards(), Is.Empty);
        });
        Assert.That(new SearchPage(Session).EmptyResultsTitleText, Does.Contain("everything"));
    }
}
