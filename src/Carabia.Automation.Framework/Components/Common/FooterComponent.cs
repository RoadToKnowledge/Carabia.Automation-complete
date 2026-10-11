using Carabia.Automation.Framework.Base;
using OpenQA.Selenium;

namespace Carabia.Automation.Framework.Components.Common;

/// <summary>Views/Shared/_FooterPartial.cshtml (footer.crb-master-footer). Selectors proven by carabia-ui-tests FooterLinkTests.</summary>
public sealed class FooterComponent(BrowserSession session) : ComponentBase(session)
{
    private static readonly By Root = By.CssSelector("footer.crb-master-footer");
    private static readonly By Logo = By.CssSelector("footer.crb-master-footer .crb-footer-brand-logo");

    public IWebElement Element => Session.Visible(Root, "the shared footer");

    public IReadOnlyList<IWebElement> Links() => Element.FindElements(By.TagName("a"));

    public IWebElement? LinkByText(string text) =>
        text.Equals("Carabia logo", StringComparison.OrdinalIgnoreCase)
            ? Element.FindElements(Logo).SingleOrDefault()
            : Element.FindElements(By.TagName("a")).SingleOrDefault(a => a.Text.Trim() == text);

    public IReadOnlyList<IWebElement> LinksMatching(string cssSelector) => Element.FindElements(By.CssSelector(cssSelector));

    public void ScrollIntoView() => Step("Footer: check links", Element);
}
