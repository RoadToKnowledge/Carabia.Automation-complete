using Carabia.Automation.Framework.Base;
using OpenQA.Selenium;

namespace Carabia.Automation.Framework.Pages.Admin;

/// <summary>
/// IdentityController.Authorization -> Views/Identity/Authorization.cshtml (admin/manager login).
/// Only inspected; the fields are NEVER filled by automation (manual authentication).
/// </summary>
public sealed class AdminLoginPage(BrowserSession session) : PageBase(session)
{
    private static readonly By IdentityField = By.CssSelector("form input[name='Identity']");
    private static readonly By PasswordField = By.CssSelector("form input[type='password']");

    protected override string Path => "/Identity/Authorization";

    public bool HasLoginForm => Elements.Exists(IdentityField) && Elements.Exists(PasswordField);
}
