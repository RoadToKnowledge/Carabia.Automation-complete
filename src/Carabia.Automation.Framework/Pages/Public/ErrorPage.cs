using Carabia.Automation.Framework.Base;
using OpenQA.Selenium;

namespace Carabia.Automation.Framework.Pages.Public;

/// <summary>
/// Status-code page (UseStatusCodePagesWithReExecute("/System/NotFoundPage") -> Views/System/NotFoundPage.cshtml):
/// #notfound, h1 "Oops...", h2 "404 - Page Not Found", link "To the main page".
/// </summary>
public sealed class ErrorPage(BrowserSession session) : PageBase(session)
{
    private static readonly By Root = By.Id("notfound");
    private static readonly By Headline = By.CssSelector("#notfound .notfound-404 h1");
    private static readonly By Message = By.CssSelector("#notfound h2");
    private static readonly By HomeLink = By.CssSelector("#notfound a[href='/']");

    public bool IsDisplayed => Elements.IsDisplayed(Root);
    public string HeadlineText => Driver.FindElement(Headline).Text.Trim();
    public string MessageText => Driver.FindElement(Message).Text.Trim();

    public void ClickHomeLink()
    {
        var link = Elements.Clickable(HomeLink, "the 'To the main page' link");
        Step("404: go to the main page", link);
        Elements.Click(link);
    }
}
