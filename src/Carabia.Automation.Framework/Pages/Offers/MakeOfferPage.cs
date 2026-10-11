using Carabia.Automation.Framework.Base;
using Carabia.Automation.Framework.Components.Common;
using Carabia.Automation.Framework.Pages.Payment;
using Carabia.Automation.Framework.Utilities;
using OpenQA.Selenium;

namespace Carabia.Automation.Framework.Pages.Offers;

/// <summary>
/// PaymentController.PaymentDetails -> Views/Payment/PaymentDetails.cshtml, route /Payment/PaymentDetails?sellingId=N
/// (anonymous access allowed; returns 404 unless the listing is Active). Screens: #page1 "Make an offer",
/// #page2 "Review offer", #page4 "Payment" (Stripe card fields), #page5 "Offer submitted"; the active screen has class "active".
/// Offer input #offerInput with data-min-offer = 75% of the asking price; #continueOfferButton ("Submit Offer")
/// stays disabled until the amount is valid; #offerValidationMessage shows errors (payment.js).
/// "Submit Offer": visitors get the login popup; signed-in clients POST /Payment/ConfirmPayment and the offer is CREATED.
/// </summary>
public sealed class MakeOfferPage(BrowserSession session) : PageBase(session)
{
    private static readonly By ActiveScreen = By.CssSelector(".screen.active");
    private static readonly By Heading = By.CssSelector(".screen.active .page-heading");
    private static readonly By CarName = By.CssSelector("#page1 .car-name");
    private static readonly By AskingPrice = By.CssSelector("#page1 .asking-price__amount");
    private static readonly By OfferInput = By.Id("offerInput");
    private static readonly By ValidationMessage = By.Id("offerValidationMessage");
    private static readonly By MonthlyEstimate = By.Id("estimatedMonthlyPayment");
    private static readonly By SubmitOffer = By.Id("continueOfferButton");
    private static readonly By OfferDisplay = By.Id("offerDisplay");
    private static readonly By SuccessTitle = By.CssSelector("#page5 .success-title");

    public LoginPopupComponent LoginPopup { get; } = new(session);

    public void OpenForSelling(int sellingId) => Session.Open($"/Payment/PaymentDetails?sellingId={sellingId}");

    public string ActiveScreenId => Driver.FindElements(ActiveScreen).FirstOrDefault()?.GetDomAttribute("id") ?? string.Empty;
    public string HeadingText => Driver.FindElements(Heading).FirstOrDefault()?.Text.Trim() ?? string.Empty;
    public string CarNameText => Session.Visible(CarName, "the car name on the offer page").Text.Trim();
    public decimal AskingPriceValue => TextParser.Number(Driver.FindElement(AskingPrice).Text);
    public decimal MinimumOffer => decimal.Parse(Driver.FindElement(OfferInput).GetDomAttribute("data-min-offer") ?? "0", System.Globalization.CultureInfo.InvariantCulture);
    public bool SubmitEnabled => Driver.FindElement(SubmitOffer).Enabled;
    public string ValidationText => Driver.FindElement(ValidationMessage).Text.Trim();
    public string MonthlyEstimateText => Driver.FindElement(MonthlyEstimate).Text.Trim();
    public string ReviewOfferText => Driver.FindElement(OfferDisplay).Text.Trim();
    public string StripeMode => StripeModeInspector.Mode(Driver);
    public bool IsCardPaymentScreenShown => Driver.FindElements(By.Id("page4")).Any(e => e.Displayed);

    public void WaitUntilLoaded() => Session.WaitFor("the 'Make an offer' screen (#page1)", _ => ActiveScreenId == "page1");

    public void EnterOffer(decimal amount)
    {
        var input = Session.Visible(OfferInput, "the offer amount field");
        Step($"Offer: enter {amount:0}", input);
        input.SendKeys(Keys.Control + "a");
        input.SendKeys(Keys.Delete);
        input.SendKeys(decimal.Round(amount).ToString(System.Globalization.CultureInfo.InvariantCulture));
    }

    public bool WaitForSubmitState(bool enabled) =>
        WaitHelper.TryUntil(Driver, d => d.FindElement(SubmitOffer).Enabled == enabled, Session.DefaultTimeout);

    public string WaitForValidationMessage() =>
        Session.WaitFor("an offer validation message", d =>
        {
            var text = d.FindElement(ValidationMessage).Text.Trim();
            return text.Length > 0 ? text : null;
        })!;

    /// <summary>
    /// Clicks "Submit Offer". For a VISITOR this only validates (POST /Payment/ValidateOffer, read-only) and opens the
    /// login popup. For a SIGNED-IN client it creates a real offer - guard with SafetyGuard (OfferWrite) first.
    /// </summary>
    public void ClickSubmitOffer()
    {
        var button = Elements.Clickable(SubmitOffer, "the Submit Offer button");
        Step("Offer: click Submit Offer", button);
        Elements.Click(button);
    }

    public string WaitForOfferSubmitted() =>
        Session.WaitFor("the 'Offer submitted' screen (#page5)", d =>
            ActiveScreenId == "page5" ? d.FindElement(SuccessTitle).Text.Trim() : null, Session.LongTimeout)!;
}
