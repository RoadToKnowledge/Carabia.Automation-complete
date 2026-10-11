using Carabia.Automation.Framework.Base;
using OpenQA.Selenium;

namespace Carabia.Automation.Framework.Pages.Public;

/// <summary>
/// InfoController.Page -> Views/Info/Page.cshtml, route /info/{alias}. The body is admin-managed HTML
/// (Administrator/Pages), so tests check the shell, title and that content is rendered - not the wording.
/// Unpublished or unknown aliases return 404.
/// </summary>
public sealed class InfoPage(BrowserSession session) : PageBase(session)
{
    private static readonly By PageContent = By.CssSelector(".page__content");

    public void OpenAlias(string alias) => Session.Open($"/info/{alias}");

    public string ContentText => Session.Visible(PageContent, "the info page content").Text.Trim();

    /// <summary>Page.cshtml (how-it-works) gives the fees heading id="our-fees".</summary>
    public bool WaitForFeesAnchor() => Utilities.WaitHelper.TryUntil(Driver, d => d.FindElements(By.Id("our-fees")).Count > 0, Session.DefaultTimeout);
    public int FeesAnchorCount => Driver.FindElements(By.Id("our-fees")).Count;
}

/// <summary>Contact page linked from the footer (/contact-us).</summary>
public sealed class ContactPage(BrowserSession session) : PageBase(session)
{
    private static readonly By PageContent = By.CssSelector(".page__content");

    protected override string Path => "/contact-us";

    public string ContentText => Session.Visible(PageContent, "the contact page content").Text.Trim();
}
