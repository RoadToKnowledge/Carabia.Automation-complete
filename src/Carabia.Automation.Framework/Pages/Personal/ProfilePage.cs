using Carabia.Automation.Framework.Base;
using Carabia.Automation.Framework.Utilities;
using OpenQA.Selenium;

namespace Carabia.Automation.Framework.Pages.Personal;

/// <summary>
/// "Settings" = PersonalController.Settings -> Views/Personal/Settings.cshtml (/personal/settings).
/// form.settings with read-only inputs FirstName, LastName, User.Email, User.Phone; each has an Edit button
/// (.js-form-control-edit inside its .form-control). Email change shows .send_email_confirmation and keeps
/// Save (button[data-type='submit']) disabled until verified. Selectors and behaviour proven by carabia-ui-tests ProfileTests.
/// </summary>
public sealed class ProfilePage(BrowserSession session) : PersonalPageBase(session)
{
    public static readonly string[] Fields = ["FirstName", "LastName", "User.Email", "User.Phone"];

    private static readonly By Form = By.CssSelector("form.settings");
    private static readonly By TitleSelector = By.CssSelector(".settings__title");
    private static readonly By SaveButton = By.CssSelector("form.settings button[data-type='submit']");
    private static readonly By SendEmailCode = By.CssSelector(".send_email_confirmation");

    protected override string Path => "/personal/settings";

    private IJavaScriptExecutor Script => (IJavaScriptExecutor)Driver;

    public override void Open()
    {
        base.Open();
        Session.Visible(Form, "the signed-in personal information form");
    }

    public string TitleText => Session.Visible(TitleSelector, "the Personal Information heading").Text.Trim();

    public IWebElement Field(string name) =>
        Session.Visible(By.CssSelector($"form.settings input[name='{name}']"), $"the profile field '{name}'");

    public string FieldValue(string name) => Field(name).GetDomProperty("value") ?? string.Empty;

    public bool IsReadOnly(string name) => Script.ExecuteScript("return arguments[0].readOnly;", Field(name)) is true;

    public IWebElement Edit(string name)
    {
        var field = Field(name);
        var control = field.FindElement(By.XPath("./ancestor::div[contains(concat(' ', normalize-space(@class), ' '), ' form-control ')][1]"));
        var button = control.FindElements(By.CssSelector(".js-form-control-edit")).FirstOrDefault()
                     ?? throw new AutomationException($"The profile field '{name}' has no Edit button.");
        Step($"Settings: edit {name}", button);
        button.Click();
        Session.WaitFor($"'{name}' to become editable", _ => !IsReadOnly(name));
        return field;
    }

    public void Replace(string name, string value)
    {
        var field = Field(name);
        field.Clear();
        field.SendKeys(value);
    }

    public bool ValueMissing(string name) => Script.ExecuteScript("return arguments[0].validity.valueMissing;", Field(name)) is true;
    public bool FormIsValid => Script.ExecuteScript("return document.querySelector('form.settings').checkValidity();") is true;

    public bool SendEmailCodeVisible => Elements.IsDisplayed(SendEmailCode);
    public IWebElement Save => Driver.FindElement(SaveButton);

    /// <summary>Clicks Save and waits for the settings form to be replaced (page reload).</summary>
    public void SaveAndWaitForReload()
    {
        var form = Driver.FindElement(Form);
        Step("Settings: SAVE (test account, restored afterwards)", Save);
        Save.Click();
        WaitHelper.ForStale(Driver, form, "the profile to reload after Save", Session.LongTimeout);
    }

    public string FormAction => Driver.FindElement(Form).GetDomProperty("action") ?? string.Empty;

    /// <summary>Captures the server-rendered values (defaultValue) of the form, used to restore the profile after a save test.</summary>
    public string CaptureOriginalForm() =>
        Script.ExecuteScript("""
            const form = document.querySelector('form.settings');
            const data = new FormData(form);
            for (const input of form.querySelectorAll('input[name]')) {
                if (['FirstName', 'LastName', 'User.Phone', 'User.Email'].includes(input.name))
                    data.set(input.name, input.defaultValue);
            }
            return JSON.stringify(Array.from(data.entries()));
            """) as string ?? throw new AutomationException("Could not capture the original profile for cleanup.");

    /// <summary>Restores a captured form through the same POST the page uses (PersonalController.Settings).</summary>
    public bool RestoreOriginalForm(string actionUrl, string originalForm) =>
        Script.ExecuteAsyncScript("""
            const [url, original, done] = arguments;
            fetch(url, { method: 'POST', credentials: 'same-origin', body: new URLSearchParams(JSON.parse(original)) })
                .then(r => r.ok ? r.json() : null).then(r => done(r?.success === true)).catch(() => done(false));
            """, actionUrl, originalForm) is true;
}
