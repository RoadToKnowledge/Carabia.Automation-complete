using Carabia.Automation.Framework.Browser;
using Carabia.Automation.Framework.Components.Common;
using Carabia.Automation.Framework.Pages.Public;
using Carabia.Automation.Framework.Utilities;
using Carabia.Automation.Tests.Base;

namespace Carabia.Automation.Tests.Public;

/// <summary>Shared header (_HeaderPartial.cshtml), desktop menu (Menus view component) and the mobile drawer.</summary>
[TestFixture]
[Category(TestCategories.Public)]
[Category(TestCategories.Regression)]
[Parallelizable(ParallelScope.Self)]
public sealed class NavigationTests : UiTestBase
{
    [Test]
    public void Visitor_Header_Shows_Logo_Sell_Button_And_Sign_In_Only()
    {
        var home = new HomePage(Session);
        home.Open();
        Assert.Multiple(() =>
        {
            Assert.That(home.Header.HasLogo, Is.True);
            Assert.That(home.Header.HasSellMyCar, Is.True);
            Assert.That(UrlHelper.PathOf(home.Header.SellMyCarHref ?? ""), Is.EqualTo("/personal/sell"));
            Assert.That(home.Header.HasSignInButton, Is.True);
            Assert.That(home.Header.HasNotificationsLink, Is.False, "Notifications icon is for signed-in users only.");
            Assert.That(home.Header.HasPersonalMenu, Is.False, "Personal menu is for signed-in users only.");
        });
    }

    [Test]
    public void Header_Sell_My_Car_Opens_The_Sell_Wizard()
    {
        new HomePage(Session).Open();
        new HeaderComponent(Session).ClickSellMyCar();
        WaitHelper.ForUrl(Session.Driver, u => u.AbsolutePath.Equals("/personal/sell", StringComparison.OrdinalIgnoreCase), "/personal/sell", Session.LongTimeout);
    }

    [Test]
    public void Header_Logo_Returns_To_Home()
    {
        new SearchPage(Session).Open();
        new HeaderComponent(Session).ClickLogo();
        WaitHelper.ForUrl(Session.Driver, u => u.AbsolutePath == "/", "the home page", Session.LongTimeout);
    }

    [Test]
    public void Desktop_Menu_Links_Are_Local_And_Respond()
    {
        var home = new HomePage(Session);
        home.Open();
        var links = home.Header.DesktopLinks()
            .Where(l => UrlHelper.IsSameHost(l.Href, Settings.BaseUrl))
            .Select(l => l.Href).Distinct().Take(25).ToList();
        Assert.That(links, Is.Not.Empty, "The desktop header menu (.crb-nav-links-wrap) has no local links.");
        using var http = NewHttpProbe();
        var failures = links.Select(l => http.GetAsync(l).GetAwaiter().GetResult()).Where(r => !r.IsSuccess).Select(r => r.ToString()).ToList();
        Assert.That(failures, Is.Empty, string.Join("\n", failures));
    }

    [Test]
    public void Mobile_Drawer_Opens_Closes_And_Contains_Menu_Sections()
    {
        WindowManager.Resize(Session.Driver, Viewport.Mobile);
        var home = new HomePage(Session);
        home.Open();
        var drawer = home.MobileNavigation;
        Assert.That(drawer.HasOpenButton, Is.True, "The mobile menu button (#crbOpenDrawerBtn) should be visible at mobile width.");
        drawer.Open();
        Assert.Multiple(() =>
        {
            Assert.That(drawer.IsOverlayActive, Is.True, "The drawer overlay should be active.");
            Assert.That(drawer.HasSellMyCar, Is.True, "The drawer should offer 'Sell My Car'.");
            Assert.That(drawer.SectionTitles(), Is.Not.Empty, "The drawer menu has no sections.");
        });
        drawer.CloseWithButton();
        Assert.That(drawer.IsOpen, Is.False);
    }

    [Test]
    public void Mobile_Drawer_Closes_When_Overlay_Is_Clicked()
    {
        WindowManager.Resize(Session.Driver, Viewport.Mobile);
        var home = new HomePage(Session);
        home.Open();
        home.MobileNavigation.Open();
        home.MobileNavigation.CloseWithOverlay();
        Assert.That(home.MobileNavigation.IsOpen, Is.False);
    }

    [Test]
    public void Mobile_Drawer_Sections_Expand_On_Tap()
    {
        WindowManager.Resize(Session.Driver, Viewport.Mobile);
        var home = new HomePage(Session);
        home.Open();
        home.MobileNavigation.Open();
        if (home.MobileNavigation.DropdownCount == 0)
            Assert.Inconclusive("The drawer has no expandable sections on this environment.");
        Assert.That(home.MobileNavigation.ToggleDropdown(0), Is.True, "The first drawer section did not expand (.cs-active).");
    }

    [Test]
    public void Mobile_Drawer_Links_Are_Local_Routes()
    {
        WindowManager.Resize(Session.Driver, Viewport.Mobile);
        var home = new HomePage(Session);
        home.Open();
        home.MobileNavigation.Open();
        var hrefs = home.MobileNavigation.LinkHrefs();
        Assert.That(hrefs, Is.Not.Empty);
        var external = hrefs.Where(h => h.StartsWith("http") && !UrlHelper.IsSameHost(h, Settings.BaseUrl)
                                        && !h.Contains("wa.me") && !h.StartsWith("mailto")).ToList();
        Assert.That(external, Is.Empty, "Unexpected external links in the drawer:\n" + string.Join("\n", external));
    }
}
