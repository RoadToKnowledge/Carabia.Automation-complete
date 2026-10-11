using Carabia.Automation.Framework.Base;
using Carabia.Automation.Framework.Utilities;
using OpenQA.Selenium;

namespace Carabia.Automation.Framework.Pages.Admin;

/// <summary>
/// Admin create/edit form (e.g. Faq/EditCategory.cshtml): a POST form with antiforgery token, inputs named after
/// the model (asp-for), "Save" submit button in .form-submit-container.
/// Opening a form and checking it is read-only; Save is only clicked by guarded destructive tests.
/// </summary>
public sealed class AdminEditFormPage(BrowserSession session) : AdminPage(session)
{
    private static readonly By PostForms = By.CssSelector(".dashboard-content form[method='post']");
    private static readonly By SaveButton = By.CssSelector(".form-submit-container button[type='submit']");
    private static readonly By AntiForgery = By.CssSelector("input[name='__RequestVerificationToken']");

    public bool HasPostForm => Elements.Exists(PostForms);
    public bool HasAntiForgeryToken => Elements.Exists(AntiForgery);
    public bool HasSaveButton => Elements.IsDisplayed(SaveButton);

    public bool HasField(string name) => Driver.FindElements(By.Name(name)).Count > 0;

    public IReadOnlyList<string> MissingFields(IEnumerable<string> names) => names.Where(n => !HasField(n)).ToList();

    public bool IsRequired(string name) =>
        Driver.FindElements(By.Name(name)).FirstOrDefault()?.GetDomAttribute("required") is not null;

    public string HiddenId => Driver.FindElements(By.CssSelector("form input[type='hidden'][name='Id']")).FirstOrDefault()?.GetDomProperty("value") ?? string.Empty;

    /// <summary>Browser-side validation of the first POST form (required fields empty -> invalid).</summary>
    public bool FormIsValid =>
        ((IJavaScriptExecutor)Driver).ExecuteScript(
            "const f = document.querySelector(\".dashboard-content form[method='post']\"); return f ? f.checkValidity() : null;") is true;

    public void Fill(string name, string value)
    {
        var input = Driver.FindElement(By.Name(name));
        input.Clear();
        input.SendKeys(value);
    }

    /// <summary>WRITE. Only used by guarded destructive tests on AUTO-marked records.</summary>
    public void Save()
    {
        var button = Elements.Clickable(SaveButton, "the Save button");
        Step("Admin: SAVE", button);
        var form = Driver.FindElement(PostForms);
        Elements.Click(button);
        WaitHelper.ForStale(Driver, form, "the form to submit and the page to reload", Session.LongTimeout);
        WaitHelper.ForDocumentReady(Driver, Session.LongTimeout);
    }
}
