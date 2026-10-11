using Carabia.Automation.Framework.Base;
using Carabia.Automation.Framework.Models.TestData;
using Carabia.Automation.Framework.Utilities;
using OpenQA.Selenium;

namespace Carabia.Automation.Framework.Pages.Payment;

/// <summary>
/// Sell-listing payment step (#sell-car-step-10 in Views/Personal/Sell.cshtml, script sell-car-payment-stripe.js):
/// totals #sellPagePaymentRows / #sellPageTotalAmount, Stripe Card Element mounted in #card-element (an iframe),
/// errors #card-errors, pay button #sellCarPaymentSubmitBtn (authorises the card, then submits the listing).
/// Card data is only ever typed when the page uses a pk_test_ key (see StripeModeInspector).
/// </summary>
public sealed class StripePaymentPage(BrowserSession session) : ComponentBase(session)
{
    private static readonly By PaymentStep = By.Id("sell-car-step-10");
    private static readonly By TotalAmount = By.Id("sellPageTotalAmount");
    private static readonly By PaymentRows = By.CssSelector("#sellPagePaymentRows .fee-row");
    private static readonly By CardElement = By.Id("card-element");
    private static readonly By CardFrame = By.CssSelector("#card-element iframe");
    private static readonly By CardErrors = By.Id("card-errors");
    private static readonly By PayButton = By.Id("sellCarPaymentSubmitBtn");

    public bool IsDisplayed => Elements.IsDisplayed(PaymentStep);
    public decimal Total => TextParser.Digits(Driver.FindElement(TotalAmount).Text);
    public int PaymentRowCount => Driver.FindElements(PaymentRows).Count;
    public bool CardElementMounted => Utilities.WaitHelper.TryUntil(Driver, d => d.FindElements(CardFrame).Count > 0, Session.LongTimeout);
    public bool PayButtonEnabled => Driver.FindElement(PayButton).Enabled;
    public string CardErrorText => Driver.FindElements(CardErrors).FirstOrDefault()?.Text.Trim() ?? string.Empty;
    public string Mode => StripeModeInspector.Mode(Driver);

    /// <summary>Types a Stripe TEST card into the Card Element iframe. Refuses unless the page key is pk_test_.</summary>
    public void EnterTestCard(StripeTestCard card)
    {
        if (Mode != "test")
            throw new InvalidOperationException($"Refusing to type card data: Stripe key mode is '{Mode}', not 'test'.");
        Session.Visible(CardElement, "the Stripe card element");
        var frame = Session.WaitFor("the Stripe card iframe", d => d.FindElements(CardFrame).FirstOrDefault(), Session.LongTimeout);
        Driver.SwitchTo().Frame(frame!);
        try
        {
            var number = Session.Visible(By.CssSelector("input[name='cardnumber']"), "the card number input");
            number.SendKeys(card.Number);
            Driver.FindElement(By.CssSelector("input[name='exp-date']")).SendKeys(card.Expiry.Replace("/", ""));
            Driver.FindElement(By.CssSelector("input[name='cvc']")).SendKeys(card.Cvc);
            var postal = Driver.FindElements(By.CssSelector("input[name='postal']")).FirstOrDefault(e => e.Displayed);
            postal?.SendKeys("12345");
        }
        finally
        {
            Driver.SwitchTo().DefaultContent();
        }
    }

    /// <summary>WRITE (authorises a TEST card and submits the listing). Guarded by callers.</summary>
    public void Pay()
    {
        var button = Elements.Clickable(PayButton, "the payment button");
        Step("Payment: authorise TEST card", button);
        Elements.Click(button);
    }
}
