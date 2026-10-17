using Carabia.Automation.Framework.Base;
using Carabia.Automation.Framework.Utilities;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace Carabia.Automation.Framework.Pages.Admin;

/// <summary>SellingOffersController.Index (status filter select[name='status'] + Filter button) and Details.</summary>
public sealed class SellingOffersPage(BrowserSession session) : AdminPage(session)
{
    private static readonly By StatusSelect = By.CssSelector(".dashboard-content form[method='get'] select[name='status']");
    private static readonly By FilterButton = By.CssSelector(".dashboard-content form[method='get'] button[type='submit']");
    private static readonly By StatusCells = By.CssSelector(".dashboard-content table.table tbody tr td:nth-child(8)");
    private static readonly By ViewLinks = By.XPath("//table[contains(@class,'table')]//a[normalize-space(.)='View']");

    public IReadOnlyList<string> StatusOptions() =>
        new SelectElement(Driver.FindElement(StatusSelect)).Options.Select(o => o.GetDomAttribute("value") ?? "").ToList();

    public void FilterBy(string status)
    {
        new SelectElement(Driver.FindElement(StatusSelect)).SelectByValue(status);
        var button = Driver.FindElement(FilterButton);
        Step($"Admin offers: filter by {(status.Length == 0 ? "All" : status)}", button);
        Elements.Click(button);
        WaitHelper.ForDocumentReady(Driver, Session.LongTimeout);
    }

    public string SelectedStatus => new SelectElement(Driver.FindElement(StatusSelect)).SelectedOption.GetDomAttribute("value") ?? "";

    public IReadOnlyList<string> RowStatuses() => Driver.FindElements(StatusCells).Select(c => c.Text.Trim()).ToList();

    public IReadOnlyList<string> DetailsHrefs() => Driver.FindElements(ViewLinks).Select(a => a.GetDomProperty("href") ?? "").ToList();
}

/// <summary>ModerationController.Next and CommentsController.Moderation: either a moderation form or the Empty view.</summary>
public sealed class ModerationPage(BrowserSession session) : AdminPage(session)
{
    private static readonly By EmptyMessage = By.CssSelector(".dashboard-content .pageheader-title.text-center");
    private static readonly By ModerationForm = By.CssSelector(".dashboard-content form[method='post']");
    private static readonly By ApproveButtons = By.CssSelector(".dashboard-content button[type='submit'][data-approve='true'], .dashboard-content button.btn-primary[type='submit']");
    private static readonly By DeclineButtons = By.CssSelector(
        ".dashboard-content button[type='submit'].btn-danger, " +
        ".dashboard-content button[type='submit'][data-approve='false']");
    private static readonly By ModerationContentArea = By.CssSelector(
        ".dashboard-content .card-body, .dashboard-content .form-group");

    public bool IsEmpty => Elements.IsDisplayed(EmptyMessage);
    public string EmptyText => Driver.FindElements(EmptyMessage).FirstOrDefault()?.Text.Trim() ?? string.Empty;
    public bool HasModerationForm => Elements.Exists(ModerationForm);
    public bool HasApproveAction => Elements.Exists(ApproveButtons);
    public bool HasDeclineAction => Elements.Exists(DeclineButtons);
    public bool HasContentArea => Elements.Exists(ModerationContentArea);
}

/// <summary>AutodataController (admin) - two import buttons .js-import-tree with data-url. Never clicked by tests.</summary>
public sealed class AutodataPage(BrowserSession session) : AdminPage(session)
{
    private static readonly By ImportButtons = By.CssSelector(".js-import-tree");
    private static readonly By Status = By.Id("importStatus");

    public IReadOnlyList<string> ImportUrls() =>
        Driver.FindElements(ImportButtons).Select(b => b.GetDomAttribute("data-url") ?? "").ToList();

    public bool StatusHidden => (Driver.FindElement(Status).GetDomAttribute("class") ?? "").Contains("d-none");
}

/// <summary>
/// Guarded CRUD on FAQ categories (FaqController.EditCategory / RemoveCategory) for AUTO-marked rows only.
/// Used solely by the Destructive admin test.
/// </summary>
public sealed class FaqAdminPage(BrowserSession session) : AdminPage(session)
{
    public const string IndexPath = "/administrator/faq";
    public const string NewCategoryPath = "/administrator/faq/editcategory";

    private static By RowContaining(string text) =>
        By.XPath($"//div[contains(@class,'dashboard-content')]//table//tr[td[normalize-space(.)='{text}']]");

    public bool HasCategoryRow(string exactName) => Driver.FindElements(RowContaining(exactName)).Count > 0;

    public string EditHrefForCategory(string exactName) =>
        Driver.FindElement(RowContaining(exactName)).FindElement(By.XPath(".//a[normalize-space(.)='Edit']")).GetDomProperty("href") ?? "";

    /// <summary>Removes ONLY the row whose name is an automation marker created by this run.</summary>
    public void RemoveCategory(string exactName)
    {
        if (!RandomDataGenerator.IsAutomationMarker(exactName))
            throw new InvalidOperationException($"Refusing to remove '{exactName}': it is not an AUTO-marked automation record.");
        var row = Driver.FindElement(RowContaining(exactName));
        var remove = row.FindElement(By.XPath(".//a[contains(@class,'remove') and normalize-space(.)='Remove']"));
        Step($"Admin FAQ: REMOVE automation category '{exactName}'", remove);
        JavaScriptHelper.AutoAcceptNextConfirm(Driver);
        Elements.Click(remove);
        Session.WaitFor($"the automation category '{exactName}' to disappear", _ =>
            UrlHelper.PathOf(Driver.Url).Equals(IndexPath, StringComparison.OrdinalIgnoreCase) && !HasCategoryRow(exactName), Session.LongTimeout);
    }
}
