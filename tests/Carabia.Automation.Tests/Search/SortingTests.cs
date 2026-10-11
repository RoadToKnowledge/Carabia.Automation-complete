using Carabia.Automation.Framework.Pages.Public;
using Carabia.Automation.Tests.Base;

namespace Carabia.Automation.Tests.Search;

/// <summary>Sidebar sort radios (input[name='crb_sort']). Ported from carabia-ui-tests.</summary>
[TestFixture]
[Category(TestCategories.Public)]
[Category(TestCategories.Search)]
[Category(TestCategories.Regression)]
[Parallelizable(ParallelScope.Self)]
public sealed class SortingTests : UiTestBase
{
    [TestCaseSource(typeof(TestData), nameof(TestData.SortCases))]
    public void Matching_Cars_Follow_The_Selected_Sort_Order(string sort, string field, bool descending)
    {
        var page = new SearchPage(Session);
        page.Open();
        page.Filters.WaitUntilReady();
        page.Filters.Sort(sort);
        var cards = page.Filters.MatchingCards();
        Assert.That(cards, Has.Count.GreaterThanOrEqualTo(2), "Sorting needs at least two cars to compare.");
        var errors = new List<string>();
        for (var i = 1; i < cards.Count; i++)
        {
            var previous = field == "price" ? cards[i - 1].Price : cards[i - 1].Mileage;
            var current = field == "price" ? cards[i].Price : cards[i].Mileage;
            if (descending ? previous < current : previous > current)
                errors.Add($"positions {i}-{i + 1}: '{cards[i - 1].Title}' {previous:N0} then '{cards[i].Title}' {current:N0}");
        }
        Assert.That(errors, Is.Empty, $"Sort '{sort}' is wrong:\n" + string.Join("\n", errors));
    }

    [Test]
    public void Default_Sort_Is_Relevance()
    {
        var page = new SearchPage(Session);
        page.Open();
        page.Filters.WaitUntilReady();
        Assert.That(page.Filters.CheckedSortValue, Is.EqualTo("newest"));
    }
}
