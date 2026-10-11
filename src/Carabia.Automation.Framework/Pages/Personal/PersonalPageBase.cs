using Carabia.Automation.Framework.Base;
using Carabia.Automation.Framework.Utilities;
using OpenQA.Selenium;

namespace Carabia.Automation.Framework.Pages.Personal;

/// <summary>
/// Shared shell of the signed-in personal area (PersonalController is [Authorize(Roles = "client")]):
/// breadcrumb label (.breadcrumbs__label), side navigation Views/Personal/_NavigationPartial.cshtml
/// (.content__nav .nav__link, active: nav__link_state_active) with My Garage, Settings, Notifications, Payments.
/// The "Log out" link (.content__extra-nav) is never clicked.
/// </summary>
public abstract class PersonalPageBase(BrowserSession session) : PageBase(session)
{
    public static readonly string[] NavigationItems = ["My Garage", "Settings", "Notifications", "Payments"];

    private static readonly By Breadcrumb = By.CssSelector(".breadcrumbs .breadcrumbs__label");
    private static readonly By NavLinks = By.CssSelector(".content__nav .nav__link");
    private static readonly By ActiveNavLink = By.CssSelector(".content__nav .nav__link_state_active");
    private static readonly By LogoutLink = By.CssSelector(".content__extra-nav a.nav__link");

    /// <summary>Opens the page and fails clearly if the session is not signed in (cookie auth redirects to "/").</summary>
    public override void Open()
    {
        base.Open();
        if (!UrlHelper.SamePath(Driver.Url, Session.Url(Path!).AbsoluteUri))
            throw new AutomationException($"{GetType().Name} did not open: the browser is at {Driver.Url}. The signed-in session may have expired - " +
                                          "delete the role file in sessions/ and run again to log in manually.");
        Session.Visible(Breadcrumb, $"the {GetType().Name} breadcrumb");
    }

    public string BreadcrumbText => Driver.FindElements(Breadcrumb).FirstOrDefault()?.Text.Trim() ?? string.Empty;

    public string ActiveNavigationText =>
        Driver.FindElements(ActiveNavLink).FirstOrDefault()?.FindElement(By.CssSelector(".nav__text")).Text.Trim() ?? string.Empty;

    public IReadOnlyList<(string Text, string Href)> NavigationLinks() =>
        Driver.FindElements(NavLinks)
            .Select(a => (a.FindElements(By.CssSelector(".nav__text")).FirstOrDefault()?.Text.Trim() ?? a.Text.Trim(), a.GetDomProperty("href") ?? string.Empty))
            .ToList();

    public bool HasLogoutLink => Elements.Exists(LogoutLink);

    public void ClickNavigation(string text)
    {
        if (text.Contains("Log out", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Logout is intentionally not automated.");
        var link = Driver.FindElements(NavLinks).FirstOrDefault(a => a.Text.Contains(text, StringComparison.OrdinalIgnoreCase))
                   ?? throw new AutomationException($"Personal navigation has no '{text}' entry.");
        Step($"Personal: open {text}", link);
        Elements.Click(link);
        WaitHelper.ForDocumentReady(Driver, Session.LongTimeout);
    }
}
