using Carabia.Automation.Framework.Base;
using OpenQA.Selenium;

namespace Carabia.Automation.Framework.Pages.Personal;

/// <summary>
/// PersonalController.Notifications -> Views/Personal/Notifications.cshtml (/personal/notifications).
/// Note: opening the page marks unread notifications as shown (server side) - a normal user side effect.
/// List: ul.log.content__log; date rows .log__date; entries li.log__item[id] with .log__panel (unread: js-new-notification).
/// </summary>
public sealed class NotificationsPage(BrowserSession session) : PersonalPageBase(session)
{
    private static readonly By Log = By.CssSelector("ul.log.content__log");
    private static readonly By Entries = By.CssSelector("ul.log.content__log li.log__item[id] .log__panel");
    private static readonly By DateRows = By.CssSelector("ul.log.content__log .log__date");

    protected override string Path => "/personal/notifications";

    public bool HasLog => Elements.Exists(Log);
    public int EntryCount => Driver.FindElements(Entries).Count;
    public int DateGroupCount => Driver.FindElements(DateRows).Count;
    public IReadOnlyList<string> EntryTexts() => Driver.FindElements(Entries).Select(e => e.Text.Trim()).ToList();
}

/// <summary>PersonalController.Payments -> Views/Personal/Payments.cshtml (/personal/payments): .table_type_payments with .table__row rows.</summary>
public sealed class PersonalPaymentsPage(BrowserSession session) : PersonalPageBase(session)
{
    public static readonly string[] ExpectedColumns = ["Date & Time", "Id", "Amount", "Sale Type", "Status"];

    private static readonly By Table = By.CssSelector(".table.table_type_payments");
    private static readonly By HeaderCells = By.CssSelector(".table_type_payments .table__header .table__cell .table__text");
    private static readonly By Rows = By.CssSelector(".table_type_payments .table__body .table__row");

    protected override string Path => "/personal/payments";

    public bool HasTable => Elements.Exists(Table);
    public IReadOnlyList<string> ColumnTitles() =>
        Driver.FindElements(HeaderCells).Select(c => (c.GetDomProperty("textContent") ?? string.Empty).Trim()).Where(t => t.Length > 0).ToList();
    public int RowCount => Driver.FindElements(Rows).Count;
}
