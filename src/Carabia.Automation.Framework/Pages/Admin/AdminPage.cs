using Carabia.Automation.Framework.Base;
using Carabia.Automation.Framework.Utilities;
using OpenQA.Selenium;

namespace Carabia.Automation.Framework.Pages.Admin;

/// <summary>
/// Generic page object for the Administrator area (Areas/Administrator/Views/Shared/_Layout.cshtml):
/// title = ViewBag.PageTitle, headings .pageheader-title / .card-header, left navigation #navbarNav .nav-link,
/// tables "table.table", Add buttons a.btn-primary with text "Add", Edit buttons, Remove links a.remove (read only).
/// </summary>
public class AdminPage(BrowserSession session) : PageBase(session)
{
    private static readonly By LeftNavLinks = By.CssSelector("#navbarNav .nav-link");
    private static readonly By CarsSubmenuToggle = By.CssSelector("#navbarNav a.nav-link[data-target='#submenu-1']");
    private static readonly By Headings = By.CssSelector(".dashboard-content .pageheader-title, .dashboard-content .card-header");
    private static readonly By Tables = By.CssSelector(".dashboard-content table.table");
    private static readonly By BodyRows = By.CssSelector(".dashboard-content table.table tbody tr");
    private static readonly By AddButtons = By.XPath("//div[contains(@class,'dashboard-content')]//a[contains(@class,'btn-primary') and normalize-space(.)='Add']");
    private static readonly By EditButtons = By.XPath("//div[contains(@class,'dashboard-content')]//a[contains(@class,'btn') and normalize-space(.)='Edit']");
    private static readonly By RemoveLinkSelector = By.XPath("//div[contains(@class,'dashboard-content')]//a[contains(@class,'btn-danger') and normalize-space(.)='Remove']");
    private static readonly By TopBrand = By.CssSelector(".dashboard-header .navbar-brand");

    public void OpenModule(AdminModule module) => Session.Open(module.Path);

    public void OpenPath(string path, string description)
    {
        Session.Open(path);
        if (!UrlHelper.PathOf(Driver.Url).StartsWith("/administrator", StringComparison.OrdinalIgnoreCase))
            throw new AutomationException($"{description} did not open; the browser was sent to {Driver.Url}. " +
                                          "The admin session may have expired, or the account lacks the role for this module.");
    }

    public bool IsInAdministratorArea => UrlHelper.PathOf(Driver.Url).StartsWith("/administrator", StringComparison.OrdinalIgnoreCase);
    public bool HasLayout => Elements.Exists(LeftNavLinks) && Elements.Exists(TopBrand);

    public IReadOnlyList<string> HeadingTexts() =>
        Driver.FindElements(Headings).Select(h => TextParser.Normalize(h.Text)).Where(t => t.Length > 0).ToList();

    public IReadOnlyList<(string Text, string Href)> LeftNavigation()
    {
        // Expand the "Cars" submenu so its links are interactable/visible.
        var toggle = Driver.FindElements(CarsSubmenuToggle).FirstOrDefault(e => e.Displayed);
        if (toggle is not null && toggle.GetDomAttribute("aria-expanded") != "true")
        {
            Elements.Click(toggle);
            WaitHelper.TryUntil(Driver, d => d.FindElements(By.CssSelector("#submenu-1.show")).Count > 0, TimeSpan.FromSeconds(5));
        }

        return Driver.FindElements(LeftNavLinks)
            .Select(a => (TextParser.Normalize(a.GetDomProperty("textContent") ?? string.Empty), a.GetDomProperty("href") ?? string.Empty))
            .Where(l => l.Item1.Length > 0)
            .ToList();
    }

    public int TableCount => Driver.FindElements(Tables).Count;
    public int RowCount => Driver.FindElements(BodyRows).Count;

    public IReadOnlyList<string> ColumnHeaders(int tableIndex = 0) =>
        Driver.FindElements(Tables).ElementAtOrDefault(tableIndex)?.FindElements(By.CssSelector("thead th"))
            .Select(th => (th.GetDomProperty("textContent") ?? string.Empty).Trim()).Where(t => t.Length > 0).ToList() ?? [];

    public IReadOnlyList<string> AddButtonHrefs() =>
        Driver.FindElements(AddButtons).Select(a => a.GetDomProperty("href") ?? string.Empty).ToList();

    public IReadOnlyList<string> EditButtonHrefs() =>
        Driver.FindElements(EditButtons).Select(a => a.GetDomProperty("href") ?? string.Empty).ToList();

    /// <summary>Remove links are counted and inspected (they must use the confirm() guard class "remove"), never clicked.</summary>
    public IReadOnlyList<(string Href, bool HasConfirmGuard)> RemoveLinks() =>
        Driver.FindElements(RemoveLinkSelector)
            .Select(a => (a.GetDomProperty("href") ?? string.Empty, (a.GetDomAttribute("class") ?? string.Empty).Split(' ').Contains("remove")))
            .ToList();

    /// <summary>Bootstrap tabs: clicks the tab and returns whether its pane became active.</summary>
    public bool ClickTab(string text)
    {
        var tabs = Driver.FindElements(By.CssSelector(".dashboard-content a.nav-link[data-toggle='tab']"));
        var tab = tabs.FirstOrDefault(a => TextParser.Normalize(a.Text).Equals(text, StringComparison.OrdinalIgnoreCase))
                  ?? tabs.FirstOrDefault(a => TextParser.Normalize(a.Text).StartsWith(text, StringComparison.OrdinalIgnoreCase))
                  ?? throw new AutomationException($"No admin tab '{text}' on {Driver.Url}.");
        Step($"Admin: open tab {text}", tab);
        Elements.Click(tab);
        var paneSelector = tab.GetDomAttribute("href") ?? string.Empty;
        return WaitHelper.TryUntil(Driver, d =>
            d.FindElements(By.CssSelector(paneSelector)).FirstOrDefault() is { } pane
            && (pane.GetDomAttribute("class") ?? string.Empty).Contains("active"), TimeSpan.FromSeconds(5));
    }

    public IReadOnlyList<string> TabTexts() =>
        Driver.FindElements(By.CssSelector(".dashboard-content a.nav-link[data-toggle='tab']"))
            .Select(a => TextParser.Normalize(a.Text)).ToList();
}
