using Carabia.Automation.Framework.Utilities;
using OpenQA.Selenium;

namespace Carabia.Automation.Framework.Base;

/// <summary>A reusable part of a page (header, footer, filter sidebar ...). Holds its own selectors.</summary>
public abstract class ComponentBase(BrowserSession session)
{
    protected BrowserSession Session { get; } = session;
    protected IWebDriver Driver => Session.Driver;
    protected ElementActions Elements => Session.Elements;
    protected TimeSpan Timeout => Session.DefaultTimeout;

    protected void Step(string message, IWebElement? element = null) => Session.Step(message, element);
}
