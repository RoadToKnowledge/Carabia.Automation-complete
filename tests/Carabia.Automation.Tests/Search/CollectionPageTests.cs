using Carabia.Automation.Framework.Pages.Public;
using Carabia.Automation.Framework.Utilities;
using Carabia.Automation.Tests.Base;

namespace Carabia.Automation.Tests.Search;

/// <summary>
/// Collection routes "sellings/{filter1:filter_constraint}" (General/SellingCollections): quick filters and
/// body-/location-/model-/under- prefixes, plus /sellings/sold (status constraint).
/// </summary>
[TestFixture]
[Category(TestCategories.Public)]
[Category(TestCategories.Search)]
[Category(TestCategories.Regression)]
[Parallelizable(ParallelScope.Self)]
public sealed class CollectionPageTests : UiTestBase
{
    [TestCaseSource(typeof(TestData), nameof(TestData.QuickFilterCollections))]
    public void Quick_Filter_Collection_Has_Heading_And_Results_Area(string slug, string headingWord)
    {
        var page = new CollectionPage(Session);
        page.OpenCollection(slug);
        Assert.Multiple(() =>
        {
            Assert.That(page.IsNotFoundPage, Is.False, $"/sellings/{slug} is not routable.");
            Assert.That(page.HeadingText, Does.Contain(headingWord).IgnoreCase);
            Assert.That(page.HasResultsArea, Is.True);
            Assert.That(page.Title, Is.Not.Empty);
        });
    }

    [TestCaseSource(typeof(TestData), nameof(TestData.PrefixCollections))]
    public void Prefix_Collection_Has_Heading_And_Results(string path, string headingWord)
    {
        var page = new CollectionPage(Session);
        page.OpenPath(path);
        Assert.Multiple(() =>
        {
            Assert.That(page.IsNotFoundPage, Is.False, $"{path} is not routable (dictionary value missing on this environment?).");
            Assert.That(page.HeadingText, Does.Contain(headingWord).IgnoreCase);
            Assert.That(page.HasResultsArea, Is.True);
        });
    }

    [Test]
    public void Under_Price_Collection_Only_Lists_Cars_Under_The_Ceiling()
    {
        var page = new CollectionPage(Session);
        page.OpenCollection("under-100000");
        var cards = page.Filters.MatchingCards();
        if (cards.Count == 0) Assert.Inconclusive("No cars under AED 100,000 on this environment.");
        var tooExpensive = cards.Where(c => c.Price > 100000).Select(c => $"{c.Title}: {c.Price:N0}").ToList();
        Assert.That(tooExpensive, Is.Empty, string.Join("\n", tooExpensive));
    }

    [Test]
    public void Model_Collection_Only_Lists_That_Model()
    {
        var landing = TestData.Search.FaqLandingPages.First(p => p.Path.StartsWith("/sellings/model-"));
        var page = new CollectionPage(Session);
        page.OpenPath(landing.Path);
        var cards = page.Filters.MatchingCards();
        if (cards.Count == 0) Assert.Inconclusive($"No {landing.ExpectedHeading} listings on this environment.");
        var wrong = cards.Where(c => !c.MatchesVehicle(landing.ExpectedHeading)).Select(c => c.Title).ToList();
        Assert.That(wrong, Is.Empty, $"{landing.Path} shows other vehicles:\n" + string.Join("\n", wrong));
    }

    [Test]
    public void Previously_Listed_Status_Page_Opens()
    {
        var page = new CollectionPage(Session);
        page.OpenPath("/sellings/sold");
        Assert.Multiple(() =>
        {
            Assert.That(page.IsNotFoundPage, Is.False);
            Assert.That(page.Title, Does.Contain("Previously Listed"));
        });
    }

    [Test]
    public async Task Unknown_Collection_Slug_Returns_404()
    {
        using var http = NewHttpProbe();
        var response = await http.GetAsync("/sellings/automation-unknown-collection");
        Assert.That(response.Code, Is.EqualTo(404), response.ToString());
    }

    [Test]
    public void Collection_Cards_Link_To_Listing_Details()
    {
        var page = new CollectionPage(Session);
        page.OpenCollection("suvs");
        var cards = page.Cards();
        if (cards.Count == 0) Assert.Inconclusive("No SUVs on this environment.");
        Assert.That(cards.All(c => UrlHelper.SellingIdFromUrl(c.Href) is not null), Is.True);
    }
}
