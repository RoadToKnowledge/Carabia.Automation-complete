using Carabia.Automation.Framework.Browser;
using Carabia.Automation.Framework.Flows;
using Carabia.Automation.Framework.Pages.Public;
using Carabia.Automation.Framework.Pages.Sell;
using Carabia.Automation.Tests.Base;

namespace Carabia.Automation.Tests.Responsive;

/// <summary>Desktop 1920x1080, tablet 768x1024, mobile 390x844 - one parameterised set of checks.</summary>
[TestFixture]
[Category(TestCategories.Public)]
[Category(TestCategories.Responsive)]
[Category(TestCategories.Regression)]
[Parallelizable(ParallelScope.Self)]
public sealed class ResponsiveTests : UiTestBase
{
    private static IEnumerable<TestCaseData> Viewports() =>
        Viewport.All.Select(v => new TestCaseData(v).SetArgDisplayNames(v.Name));

    private static IEnumerable<TestCaseData> ViewportsAndPages() =>
        from v in Viewport.All
        from p in new[] { "/", "/sellings", "/personal/sell", "/sitemap", "/info/about-us", "/brand/bmw", "/news", "/info/how-it-works", "/contact-us" }
        select new TestCaseData(v, p).SetArgDisplayNames(v.Name, p);

    [TestCaseSource(nameof(ViewportsAndPages))]
    public void Page_Has_No_Horizontal_Scroll(Viewport viewport, string path)
    {
        WindowManager.Resize(Session.Driver, viewport);
        Session.Open(path);
        Assert.That(WindowManager.HasHorizontalOverflow(Session.Driver), Is.False, $"{path} scrolls sideways at {viewport}.");
    }

    [TestCaseSource(nameof(Viewports))]
    public void Header_Shows_Desktop_Menu_Or_Mobile_Drawer_Button(Viewport viewport)
    {
        WindowManager.Resize(Session.Driver, viewport);
        var home = new HomePage(Session);
        home.Open();
        var mobileLayout = WindowManager.InnerWidth(Session.Driver) <= 1024; // drawer breakpoint is CSS-driven
        Assert.Multiple(() =>
        {
            Assert.That(home.Header.HasLogo, Is.True);
            Assert.That(home.Header.HasDesktopNavigation || home.MobileNavigation.HasOpenButton, Is.True,
                "Neither the desktop menu nor the mobile drawer button is visible.");
            if (viewport == Viewport.Desktop)
                Assert.That(home.Header.HasDesktopNavigation, Is.True, "Desktop should show the header menu.");
            if (viewport == Viewport.Mobile)
                Assert.That(home.MobileNavigation.HasOpenButton, Is.True, "Mobile should show the drawer button.");
        });
        TestContext.Out.WriteLine($"{viewport}: innerWidth={WindowManager.InnerWidth(Session.Driver)} mobileLayout~{mobileLayout}");
    }

    [TestCaseSource(nameof(Viewports))]
    public void Hero_Search_Is_Usable(Viewport viewport)
    {
        WindowManager.Resize(Session.Driver, viewport);
        var home = new HomePage(Session);
        home.Open();
        Assert.That(home.HasHeroSearch, Is.True);
    }

    [TestCaseSource(nameof(Viewports))]
    public void Listing_Cards_Render(Viewport viewport)
    {
        WindowManager.Resize(Session.Driver, viewport);
        var page = new SearchPage(Session);
        page.Open();
        Assert.That(page.WaitForCards().Count(c => c.Root.Displayed), Is.GreaterThan(0));
    }

    [TestCaseSource(nameof(Viewports))]
    public void Listing_Details_Show_Price_And_Gallery(Viewport viewport)
    {
        WindowManager.Resize(Session.Driver, viewport);
        new BrowseCarsFlow(Session).OpenListingFromResults();
        var details = new ListingDetailsPage(Session);
        Assert.Multiple(() =>
        {
            Assert.That(details.Gallery.HasMainImage, Is.True);
            // Price is rendered twice (.lps-price-mobile and .lps-price-desk); one of them must be visible.
            Assert.That(details.AnyPriceVisible, Is.True);
        });
    }

    [TestCaseSource(nameof(Viewports))]
    public void Sell_Wizard_Step_One_Is_Usable(Viewport viewport)
    {
        WindowManager.Resize(Session.Driver, viewport);
        var sell = new SellCarPage(Session);
        sell.Open();
        Assert.That(sell.Vehicle.Field(Framework.Components.Sell.VehicleSelectorComponent.VinInput).Displayed, Is.True);
    }
}
