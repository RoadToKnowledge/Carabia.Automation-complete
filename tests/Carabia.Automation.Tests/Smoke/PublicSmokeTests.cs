using Carabia.Automation.Framework.Pages.Public;
using Carabia.Automation.Framework.Pages.Sell;
using Carabia.Automation.Tests.Base;

namespace Carabia.Automation.Tests.Smoke;

/// <summary>Fast, read-only health check of the main public pages. Safe for CI and production.</summary>
[TestFixture]
[Category(TestCategories.Smoke)]
[Category(TestCategories.Public)]
[Parallelizable(ParallelScope.Self)]
public sealed class PublicSmokeTests : UiTestBase
{
    [TestCaseSource(typeof(TestData), nameof(TestData.PublicRoutes))]
    public async Task Public_Route_Returns_Http_Success(string path)
    {
        using var http = NewHttpProbe();
        var result = await http.GetAsync(path);
        Assert.That(result.IsSuccess, Is.True, result.ToString());
    }

    [TestCaseSource(typeof(TestData), nameof(TestData.PublicRoutes))]
    public void Public_Route_Renders_Visible_Content_In_Browser(string path)
    {
        var page = new GenericPage(Session);
        page.OpenPath(path);
        Assert.Multiple(() =>
        {
            Assert.That(page.ShowsBrowserErrorPage, Is.False, $"{path}: the browser shows a connection error.");
            Assert.That(page.IsNotFoundPage, Is.False, $"{path}: Carabia rendered its 404 page.");
            Assert.That(page.BodyText.Trim(), Is.Not.Empty, $"{path}: the page body is empty.");
        });
    }

    [Test]
    public void Home_Page_Shows_Header_Hero_Search_And_Footer()
    {
        var home = new HomePage(Session);
        home.Open();
        Assert.Multiple(() =>
        {
            Assert.That(home.Header.IsDisplayed, Is.True, "Header #crbDynamicHeader is missing.");
            Assert.That(home.Header.HasSellMyCar, Is.True, "Header 'Sell My Car' button is missing.");
            Assert.That(home.HasHeroSearch, Is.True, "Hero search (make, model, Search) is missing.");
            Assert.That(home.Footer.Links(), Is.Not.Empty, "Footer has no links.");
        });
    }

    [Test]
    public void Listings_Page_Shows_Results_And_Filters()
    {
        var search = new SearchPage(Session);
        search.Open();
        Assert.Multiple(() =>
        {
            Assert.That(search.Filters.IsDisplayed, Is.True, "Filter sidebar is missing.");
            Assert.That(search.ResultCount, Is.GreaterThan(0), "DEV must have at least one active listing.");
            Assert.That(search.WaitForCards(), Is.Not.Empty);
        });
    }

    [Test]
    public void Sell_My_Car_Wizard_Opens_On_Vehicle_Step_For_Visitors()
    {
        var sell = new SellCarPage(Session);
        sell.Open();
        Assert.That(sell.Wizard.CurrentStep(), Is.EqualTo(1));
    }
}
