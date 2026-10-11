using Carabia.Automation.Framework.Base;
using OpenQA.Selenium;

namespace Carabia.Automation.Framework.Components.Common;

/// <summary>Views/Shared/_HeaderPartial.cshtml (desktop header).</summary>
public sealed class HeaderComponent(BrowserSession session) : ComponentBase(session)
{
    private static readonly By Root = By.Id("crbDynamicHeader");
    private static readonly By Logo = By.CssSelector("#crbDynamicHeader .crb-main-nav > a.crb-brand-logo");
    private static readonly By SellMyCar = By.CssSelector("#crbDynamicHeader .crb-nav-right-actions .crb-btn-sell-car");
    private static readonly By DesktopNavigation = By.CssSelector("#crbDynamicHeader .crb-nav-links-wrap");
    private static readonly By DesktopNavigationLinks = By.CssSelector("#crbDynamicHeader .crb-nav-links-wrap a[href]");
    private static readonly By SignInButton = By.Id("profileBtn");
    private static readonly By NotificationsLink = By.CssSelector("#crbDynamicHeader a.crb-icon-action-btn[aria-label='Notifications']");
    private static readonly By PersonalMenuButton = By.CssSelector("#crbDynamicHeader button[data-for-dropdown='personal']");
    private static readonly By PersonalMenuLinks = By.CssSelector(".crb-profile-menu-list a");
    private static readonly By MobileMenuButton = By.Id("crbOpenDrawerBtn");

    public bool IsDisplayed => Elements.IsDisplayed(Root);
    public bool HasLogo => Elements.IsDisplayed(Logo);
    public bool HasSellMyCar => Elements.IsDisplayed(SellMyCar);
    public bool HasDesktopNavigation => Elements.IsDisplayed(DesktopNavigation);
    public bool HasSignInButton => Elements.IsDisplayed(SignInButton);
    public bool HasNotificationsLink => Elements.IsDisplayed(NotificationsLink);
    public bool HasPersonalMenu => Elements.Exists(PersonalMenuButton);
    public bool HasMobileMenuButton => Elements.IsDisplayed(MobileMenuButton);

    public string? SellMyCarHref => Driver.FindElements(SellMyCar).FirstOrDefault()?.GetDomProperty("href");

    public IReadOnlyList<(string Text, string Href)> DesktopLinks() =>
        Driver.FindElements(DesktopNavigationLinks)
            .Select(a => (Text: (a.GetDomProperty("textContent") ?? string.Empty).Trim(), Href: a.GetDomProperty("href") ?? string.Empty))
            .Where(l => !string.IsNullOrWhiteSpace(l.Href))
            .ToList();

    public void ClickLogo()
    {
        var logo = Elements.Clickable(Logo, "the header logo");
        Step("Header: click logo", logo);
        Elements.Click(logo);
    }

    public void ClickSellMyCar()
    {
        var button = Elements.Clickable(SellMyCar, "the header 'Sell My Car' button");
        Step("Header: click Sell My Car", button);
        Elements.Click(button);
    }

    public void OpenNotifications()
    {
        var link = Elements.Clickable(NotificationsLink, "the header notifications icon (authenticated only)");
        Step("Header: open notifications", link);
        Elements.Click(link);
    }

    /// <summary>Opens the authenticated personal dropdown and returns its links (My Garage, Settings, Notifications, Payments, Log out).</summary>
    public IReadOnlyList<(string Text, string Href)> OpenPersonalMenu()
    {
        var button = Elements.Clickable(PersonalMenuButton, "the personal menu button (authenticated only)");
        Step("Header: open personal menu", button);
        Elements.Click(button);
        Session.WaitFor("the personal menu links to be visible", d => d.FindElements(PersonalMenuLinks).Any(e => e.Displayed));
        return Driver.FindElements(PersonalMenuLinks)
            .Select(a => (Text: a.Text.Trim(), Href: a.GetDomProperty("href") ?? string.Empty))
            .ToList();
    }

    /// <summary>Clicks a personal-menu entry by its text. Refuses "Log out": logout automation is out of scope.</summary>
    public void ClickPersonalMenuItem(string text)
    {
        if (text.Contains("log out", StringComparison.OrdinalIgnoreCase) || text.Contains("logout", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Logout is intentionally not automated. Use a fresh browser for a clean state.");
        OpenPersonalMenu();
        var link = Session.WaitFor($"personal menu item '{text}'", d =>
            d.FindElements(PersonalMenuLinks).FirstOrDefault(a => a.Displayed && a.Text.Trim().Equals(text, StringComparison.OrdinalIgnoreCase)));
        Elements.Click(link!);
    }
}
