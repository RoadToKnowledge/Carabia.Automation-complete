using Carabia.Automation.Framework.Components.Common;
using Carabia.Automation.Framework.Flows;
using Carabia.Automation.Framework.Pages.Personal;
using Carabia.Automation.Framework.Pages.Public;
using Carabia.Automation.Framework.Utilities;
using Carabia.Automation.Tests.Base;

namespace Carabia.Automation.Tests.Personal;

/// <summary>Signed-in Buyer: authenticated header, personal navigation, My Garage tabs, favourites, notifications, payments.</summary>
[TestFixture]
[Category(TestCategories.Personal)]
[Category(TestCategories.Regression)]
public sealed class BuyerPersonalAreaTests : BuyerTestBase
{
    [Test]
    public void Signed_In_Header_Shows_Notifications_And_Personal_Menu_Instead_Of_Sign_In()
    {
        var home = new HomePage(Session);
        home.Open();
        Assert.Multiple(() =>
        {
            Assert.That(home.Header.HasSignInButton, Is.False);
            Assert.That(home.Header.HasNotificationsLink, Is.True);
            Assert.That(home.Header.HasPersonalMenu, Is.True);
        });
    }

    [Test]
    public void Personal_Menu_Lists_Garage_Settings_Notifications_Payments()
    {
        var home = new HomePage(Session);
        home.Open();
        var paths = home.Header.OpenPersonalMenu().Select(l => UrlHelper.PathOf(l.Href).ToLowerInvariant()).ToList();
        Assert.That(paths, Is.SupersetOf(new[] { "/personal/buying", "/personal/settings", "/personal/notifications", "/personal/payments" }));
        // "Log out" is present in the menu but is never clicked by automation.
    }

    [TestCase("Settings", "/personal/settings")]
    [TestCase("Payments", "/personal/payments")]
    public void Personal_Menu_Item_Opens_Its_Page(string item, string path)
    {
        new HomePage(Session).Open();
        var header = new HeaderComponent(Session);
        header.ClickPersonalMenuItem(item);
        WaitHelper.ForUrl(Session.Driver, u => u.AbsolutePath.Equals(path, StringComparison.OrdinalIgnoreCase), path, Session.LongTimeout);
    }

    [Test]
    public void Header_Bell_Opens_Notifications()
    {
        new HomePage(Session).Open();
        new HeaderComponent(Session).OpenNotifications();
        WaitHelper.ForUrl(Session.Driver, u => u.AbsolutePath.Equals("/personal/notifications", StringComparison.OrdinalIgnoreCase),
            "/personal/notifications", Session.LongTimeout);
    }

    [Test]
    public void My_Garage_Shows_Three_Tabs_And_Buying_Is_Active()
    {
        var garage = new MyGaragePage(Session);
        garage.OpenTab(MyGaragePage.GarageTab.Buying);
        Assert.Multiple(() =>
        {
            Assert.That(garage.TabTexts(), Is.EqualTo(new[] { "Buying", "Selling", "Favourites" }));
            Assert.That(garage.ActiveTabText, Is.EqualTo("Buying"));
            Assert.That(garage.BreadcrumbText, Is.EqualTo("My Garage"));
            Assert.That(garage.ActiveNavigationText, Is.EqualTo("My Garage"));
        });
    }

    [TestCase("Selling", "/personal/selling")]
    [TestCase("Favourites", "/personal/favorites")]
    [TestCase("Buying", "/personal/buying")]
    public void My_Garage_Tab_Navigates(string tab, string path)
    {
        var garage = new MyGaragePage(Session);
        garage.OpenTab(MyGaragePage.GarageTab.Buying);
        garage.ClickTab(tab);
        Assert.Multiple(() =>
        {
            Assert.That(UrlHelper.PathOf(Session.Driver.Url), Is.EqualTo(path).IgnoreCase);
            Assert.That(garage.ActiveTabText, Is.EqualTo(tab));
        });
    }

    [Test]
    public void Personal_Navigation_Has_All_Sections_And_Logout_Is_Not_Used()
    {
        var garage = new MyGaragePage(Session);
        garage.OpenTab(MyGaragePage.GarageTab.Buying);
        Assert.Multiple(() =>
        {
            Assert.That(garage.NavigationLinks().Select(l => l.Text), Is.EqualTo(PersonalPageBase.NavigationItems));
            Assert.That(garage.HasLogoutLink, Is.True, "The Log out link exists (it is intentionally never clicked).");
        });
    }

    [Test]
    public void Adding_And_Removing_A_Favourite_Updates_The_Favourites_Tab()
    {
        // Favourites live only in the browser cookie "carabia_favorites" (custom.js); the test removes what it adds.
        var (_, url) = new BrowseCarsFlow(Session).OpenListingFromResults();
        var details = new ListingDetailsPage(Session);
        var sellingId = details.SellingId!.Value;
        var wasAdded = details.WishlistIsAdded;
        if (!wasAdded) details.ClickWishlist();
        Session.WaitFor("the heart to show 'added'", _ => details.WishlistIsAdded);

        var garage = new MyGaragePage(Session);
        garage.OpenTab(MyGaragePage.GarageTab.Favourites);
        Assert.That(garage.CardTitles().Count + garage.CardElements().Count, Is.GreaterThan(0), "Favourites tab shows no listing.");

        details.OpenListing(url);
        if (!wasAdded)
        {
            details.ClickWishlist();
            Session.WaitFor("the heart to show 'not added'", _ => !details.WishlistIsAdded);
        }
        TestContext.Out.WriteLine($"Favourite toggled for listing {sellingId} and restored.");
    }

    [Test]
    public void Notifications_Page_Shows_Log()
    {
        var page = new NotificationsPage(Session);
        page.Open();
        Assert.Multiple(() =>
        {
            Assert.That(page.HasLog, Is.True, "ul.log.content__log is missing.");
            Assert.That(page.BreadcrumbText, Is.EqualTo("Notifications"));
            Assert.That(page.ActiveNavigationText, Is.EqualTo("Notifications"));
            if (page.EntryCount > 0)
                Assert.That(page.DateGroupCount, Is.GreaterThan(0), "Notifications must be grouped by date.");
        });
    }

    [Test]
    public void Payments_Page_Shows_Payment_History_Table()
    {
        var page = new PersonalPaymentsPage(Session);
        page.Open();
        Assert.Multiple(() =>
        {
            Assert.That(page.HasTable, Is.True);
            Assert.That(page.ColumnTitles(), Is.SupersetOf(PersonalPaymentsPage.ExpectedColumns));
            Assert.That(page.ActiveNavigationText, Is.EqualTo("Payments"));
            Assert.That(page.BreadcrumbText, Is.EqualTo("Payments"));
        });
    }

    [Test]
    [Category("KnownIssue")]
    public void Payments_Page_Title_Is_Payments()
    {
        // Views/Personal/Payments.cshtml sets ViewBag.PageTitle = "Notifications" (copy/paste). Documented in KNOWN_LIMITATIONS.md.
        var page = new PersonalPaymentsPage(Session);
        page.Open();
        Assert.That(page.Title, Is.EqualTo("Payments"));
    }
}
