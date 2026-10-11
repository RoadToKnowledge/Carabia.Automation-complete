using Carabia.Automation.Framework.Base;
using Carabia.Automation.Framework.Utilities;
using OpenQA.Selenium;

namespace Carabia.Automation.Framework.Components.Common;

/// <summary>
/// The three FAQ markups Carabia uses (ported from carabia-ui-tests FaqChecks):
///   1. .crb-faq-section .crb-faq-item   (checkbox toggle .crb-faq-toggle, .crb-faq-question/.crb-faq-answer)
///   2. details.crb-seo-faq__item        (native details/summary, .crb-seo-faq__answer)
///   3. .crb-seo-content .js-accordion   (.js-accordion-trigger, .accordion__body, accordion_state_open)
/// </summary>
public sealed class FaqComponent(BrowserSession session) : ComponentBase(session)
{
    private static readonly By Items = By.CssSelector(".crb-faq-section .crb-faq-item, .crb-seo-faq__item, .crb-seo-content .js-accordion");

    public int Count => Driver.FindElements(Items).Count;

    /// <summary>Clicks the n-th question open, checks a complete non-empty answer, clicks it closed. Returns the question text.</summary>
    public string OpenAndCloseQuestion(int index)
    {
        var script = (IJavaScriptExecutor)Driver;
        var item = Driver.FindElements(Items)[index];
        var isNative = item.TagName.Equals("details", StringComparison.OrdinalIgnoreCase);
        var isCheckbox = item.FindElements(By.CssSelector(".crb-faq-toggle")).Count > 0;
        var trigger = item.FindElement(By.CssSelector(isNative ? "summary" : isCheckbox ? ".crb-faq-question" : ".js-accordion-trigger"));
        var answer = item.FindElement(By.CssSelector(isNative ? ".crb-seo-faq__answer" : isCheckbox ? ".crb-faq-answer" : ".accordion__body"));
        var question = trigger.Text.Trim();
        if (string.IsNullOrWhiteSpace(question))
            throw new AutomationException($"FAQ {index + 1}: the question text is empty.");

        bool IsOpen() => isNative
            ? script.ExecuteScript("return arguments[0].open;", item) is true
            : isCheckbox ? item.FindElement(By.CssSelector(".crb-faq-toggle")).Selected
            : script.ExecuteScript("return arguments[0].classList.contains('accordion_state_open');", item) is true;

        Step($"FAQ {index + 1}: open '{question}'", trigger);
        if (IsOpen())
        {
            trigger.Click();
            Session.WaitFor($"FAQ '{question}': the initially open answer to close", _ => !IsOpen() && !JavaScriptHelper.IsReallyVisible(Driver, answer), TimeSpan.FromSeconds(5));
        }

        trigger.Click();
        Session.WaitFor($"FAQ '{question}': the complete answer to be shown", _ =>
            IsOpen() && JavaScriptHelper.IsReallyVisible(Driver, answer) && !string.IsNullOrWhiteSpace(answer.Text)
            && script.ExecuteScript("return arguments[0].scrollHeight <= arguments[0].clientHeight + 2;", answer) is true,
            TimeSpan.FromSeconds(5));

        Step($"FAQ {index + 1}: close", trigger);
        trigger.Click();
        Session.WaitFor($"FAQ '{question}': the answer to close again", _ => !IsOpen() && !JavaScriptHelper.IsReallyVisible(Driver, answer), TimeSpan.FromSeconds(5));
        return question;
    }

    /// <summary>Checks the first <paramref name="sample"/> questions and returns one failure line per broken question.</summary>
    public IReadOnlyList<string> CheckSample(string pageName, int sample = 3)
    {
        var count = Count;
        if (count == 0)
            return [$"{pageName}: no FAQ questions were found."];

        var failures = new List<string>();
        for (var i = 0; i < Math.Min(sample, count); i++)
        {
            try
            {
                var question = OpenAndCloseQuestion(i);
                Session.Log($"PASS: {pageName} FAQ {i + 1} '{question}' opens and closes.");
            }
            catch (Exception exception) when (exception is AutomationException or WebDriverException)
            {
                failures.Add($"{pageName} FAQ {i + 1}: {exception.Message}");
            }
        }
        return failures;
    }
}
