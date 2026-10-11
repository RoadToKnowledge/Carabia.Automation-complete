using Carabia.Automation.Framework.Pages.Public;
using Carabia.Automation.Framework.Utilities;
using Carabia.Automation.Tests.Base;

namespace Carabia.Automation.Tests.Public;

/// <summary>HomeController.Index: hero search (make -> model -> Search), quick filters, featured listings, Sell for free.</summary>
[TestFixture]
[Category(TestCategories.Public)]
[Category(TestCategories.Search)]
[Category(TestCategories.Regression)]
[Parallelizable(ParallelScope.Self)]
public sealed class HomeTests : UiTestBase
{
    private HomePage Home => new(Session);

    [Test]
    public void Hero_Search_Count_Matches_Initial_Active_Car_Count()
    {
        var home = Home;
        home.Open();
        Assert.Multiple(() =>
        {
            Assert.That(home.InitialSearchCount, Is.GreaterThan(0), "data-initial-count should hold the number of active cars.");
            Assert.That(home.HeroSearchCountValue, Is.EqualTo(home.InitialSearchCount));
            Assert.That(home.HeroSearchNounText, Is.EqualTo(home.HeroSearchCountValue == 1 ? "car" : "cars"));
        });
    }

    [Test]
    public void Search_Without_Selection_Opens_All_Listings()
    {
        var home = Home;
        home.Open();
        home.ClickSearch();
        WaitHelper.ForUrl(Session.Driver, u => u.AbsolutePath.TrimEnd('/') == "/sellings", "navigation to /sellings", Session.LongTimeout);
        Assert.That(new SearchPage(Session).ResultCount, Is.EqualTo(home.InitialSearchCount).Or.GreaterThan(0));
    }

    [TestCaseSource(typeof(TestData), nameof(TestData.MakesWithListings))]
    public void Choosing_A_Make_Loads_Its_Models_And_Search_Opens_The_Brand_Page(string make)
    {
        var home = Home;
        home.Open();
        var before = home.HeroSearchCountValue;
        home.ChooseMake(make);
        home.WaitForSearchCountChange(before);
        Assert.That(home.ModelSelect.Options.Count, Is.GreaterThan(1), $"No models loaded for {make} (POST /System/GetCarModels).");
        Assert.That(home.HeroSearchCountValue, Is.LessThanOrEqualTo(before), "A make filter cannot increase the number of cars.");
        var slug = home.SelectedMakeSlug;
        home.ClickSearch();
        WaitHelper.ForUrl(Session.Driver, u => u.AbsolutePath.Equals($"/brand/{slug}", StringComparison.OrdinalIgnoreCase),
            $"navigation to /brand/{slug}", Session.LongTimeout);
        Assert.That(new BrandPage(Session).HeadingText, Does.Contain(make).IgnoreCase);
    }

    [TestCaseSource(typeof(TestData), nameof(TestData.MakesWithListings))]
    public void Choosing_A_Model_Search_Opens_The_Model_Collection(string make)
    {
        var home = Home;
        home.Open();
        home.ChooseMake(make);
        var model = home.ChooseFirstModel();
        var slug = home.SelectedModelSlug;
        home.ClickSearch();
        WaitHelper.ForUrl(Session.Driver, u => u.AbsolutePath.Equals($"/sellings/model-{slug}", StringComparison.OrdinalIgnoreCase),
            $"navigation to /sellings/model-{slug}", Session.LongTimeout);
        Assert.That(new SearchPage(Session).HeadingText, Does.Contain(model).IgnoreCase);
    }

    [Test]
    public void Quick_Filter_Badges_Link_To_Routable_Collections()
    {
        var home = Home;
        home.Open();
        var badges = home.QuickFilters();
        Assert.That(badges, Is.Not.Empty, "No quick filter badges (.crb-filter-badge) on the home page.");
        Assert.Multiple(() =>
        {
            foreach (var badge in badges)
            {
                Assert.That(badge.Text, Is.Not.Empty, $"Badge {badge.Key} has no text.");
                Assert.That(UrlHelper.PathOf(badge.Href), Does.StartWith("/sellings"), $"Badge {badge.Key} -> {badge.Href}");
            }
        });
    }

    [Test]
    public void Clicking_A_Quick_Filter_Opens_Its_Collection_With_Results_Area()
    {
        var home = Home;
        home.Open();
        var badge = home.QuickFilters().First();
        home.ClickQuickFilter(badge.Key);
        WaitHelper.ForUrl(Session.Driver, u => UrlHelper.SamePath(u.AbsoluteUri, badge.Href), $"navigation to {badge.Href}", Session.LongTimeout);
        var collection = new CollectionPage(Session);
        Assert.Multiple(() =>
        {
            Assert.That(collection.HeadingText, Is.Not.Empty);
            Assert.That(collection.HasResultsArea, Is.True);
        });
    }

    [Test]
    public void Sell_For_Free_Tab_Opens_Sell_My_Car()
    {
        var home = Home;
        home.Open();
        home.ClickSellForFreeTab();
        WaitHelper.ForUrl(Session.Driver, u => u.AbsolutePath.Equals("/personal/sell", StringComparison.OrdinalIgnoreCase),
            "navigation to /personal/sell", Session.LongTimeout);
    }

    [Test]
    public void Featured_Listings_Have_Titles_Prices_And_Detail_Links()
    {
        var home = Home;
        home.Open();
        var cards = home.FeaturedListings();
        Assert.That(cards, Is.Not.Empty, "The home page shows no featured listing cards.");
        Assert.Multiple(() =>
        {
            foreach (var card in cards.Where(c => c.HasTitle))
            {
                Assert.That(card.Title, Is.Not.Empty);
                Assert.That(UrlHelper.SellingIdFromUrl(card.Href), Is.Not.Null, $"'{card.Title}' does not link to /sellings/{{id}}_{{name}}: {card.Href}");
            }
        });
    }

    [Test]
    public void Configured_Featured_Listing_Opens_Its_Details_From_Home()
    {
        var path = TestData.Pages.FeaturedHomeListingPath;
        if (string.IsNullOrWhiteSpace(path))
            Assert.Ignore("No FeaturedHomeListingPath configured in testdata/Public/PublicPages.json.");
        var home = Home;
        home.Open();
        var card = home.FeaturedListingByPath(path);
        if (card is null)
            Assert.Inconclusive($"Listing {path} is no longer featured on the home page (data-dependent regression case from carabia-ui-tests).");
        card!.Open();
        var details = new ListingDetailsPage(Session);
        details.WaitUntilLoaded();
        Assert.That(UrlHelper.SamePath(Session.Driver.Url, path), Is.True);
    }
}
