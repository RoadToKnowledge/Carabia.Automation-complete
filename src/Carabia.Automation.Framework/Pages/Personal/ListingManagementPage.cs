using Carabia.Automation.Framework.Base;
using Carabia.Automation.Framework.Utilities;
using OpenQA.Selenium;

namespace Carabia.Automation.Framework.Pages.Personal;

/// <summary>
/// Seller dialogs opened from My Garage (Views/Shared/_ModalPartial.cshtml):
///  #modalOffer          "Offer Received": .accept_button, .decline_button, counter form .make_counter_offer_form
///                       input[name='MakeOfferSum'] + .make_counter_offer_submit (garage.js -> /Personal/AcceptOffer, DeclineOffer, MakeOffer)
///  #modalEditPrice      input[name='Sum'], agreement checkbox, .edit_price_submit (-> /Personal/EditPrice)
///  #modalRemoveListing  select[name='Reason'], agreement, .remove_submit (-> /Personal/AddCarDeletionRequest)
/// Every submit here WRITES data; tests only call them behind SafetyGuard and only for the automation listing.
/// </summary>
public sealed class ListingManagementPage(BrowserSession session) : ComponentBase(session)
{
    private static readonly By OfferModal = By.Id("modalOffer");
    private static readonly By OfferBuyer = By.CssSelector("#modalOffer .login");
    private static readonly By OfferSum = By.CssSelector("#modalOffer .offer_sum");
    private static readonly By AcceptButton = By.CssSelector("#modalOffer .accept_button");
    private static readonly By DeclineButton = By.CssSelector("#modalOffer .decline_button");
    private static readonly By CounterInput = By.CssSelector(".make_counter_offer_form input[name='MakeOfferSum']");
    private static readonly By CounterSubmit = By.CssSelector(".make_counter_offer_submit");

    private static readonly By EditPriceModal = By.Id("modalEditPrice");
    private static readonly By EditPriceSum = By.CssSelector("#modalEditPrice input[name='Sum']");
    private static readonly By EditPriceAgreement = By.CssSelector("#modalEditPrice input[name='agreement']");
    private static readonly By EditPriceSubmit = By.CssSelector("#modalEditPrice .edit_price_submit");

    private static readonly By RemoveModal = By.Id("modalRemoveListing");
    private static readonly By RemoveReason = By.CssSelector("#modalRemoveListing select[name='Reason']");
    private static readonly By RemoveSubmit = By.CssSelector("#modalRemoveListing .remove_submit");
    private static readonly By RemoveCancel = By.CssSelector("#modalRemoveListing .js-modal-close");

    public bool IsOfferDialogOpen => Elements.IsDisplayed(OfferModal);
    public string OfferBuyerText => Driver.FindElement(OfferBuyer).Text.Trim();
    public decimal OfferSumValue => TextParser.Number(Driver.FindElement(OfferSum).Text);
    public bool IsEditPriceOpen => Elements.IsDisplayed(EditPriceModal);
    public bool IsRemoveDialogOpen => Elements.IsDisplayed(RemoveModal);

    public void WaitForOfferDialog() =>
        Session.WaitFor("the 'Offer Received' dialog with the offer amount", _ =>
            IsOfferDialogOpen && Driver.FindElement(OfferSum).Text.Trim().Length > 0, Session.LongTimeout);

    public void WaitForEditPriceDialog() => Session.WaitFor("the Edit price dialog", _ => IsEditPriceOpen);
    public void WaitForRemoveDialog() => Session.WaitFor("the Remove listing dialog", _ => IsRemoveDialogOpen);

    public bool RemoveDialogHasReasons => Driver.FindElements(RemoveReason).Count > 0;
    public bool EditPriceHasSumField => Driver.FindElements(EditPriceSum).Count > 0;

    public void CancelRemoveDialog()
    {
        var cancel = Driver.FindElements(RemoveCancel).FirstOrDefault(e => e.Displayed);
        if (cancel is not null) Elements.Click(cancel);
        else new OpenQA.Selenium.Interactions.Actions(Driver).SendKeys(Keys.Escape).Perform();
        Session.WaitFor("the Remove listing dialog to close", _ => !IsRemoveDialogOpen);
    }

    public void CloseAnyDialog()
    {
        new OpenQA.Selenium.Interactions.Actions(Driver).SendKeys(Keys.Escape).Perform();
    }

    // ---- WRITE operations (guarded by callers) ----

    public void AcceptOffer() => ClickAndWaitForAjax(AcceptButton, "Seller: ACCEPT offer");
    public void DeclineOffer() => ClickAndWaitForAjax(DeclineButton, "Seller: DECLINE offer");

    public void CounterOffer(decimal amount)
    {
        Elements.Type(CounterInput, decimal.Round(amount).ToString(System.Globalization.CultureInfo.InvariantCulture));
        ClickAndWaitForAjax(CounterSubmit, $"Seller: COUNTER-OFFER {amount:0}");
    }

    public void SubmitNewPrice(decimal price)
    {
        Elements.Type(EditPriceSum, decimal.Round(price).ToString(System.Globalization.CultureInfo.InvariantCulture));
        var agreement = Driver.FindElement(EditPriceAgreement);
        if (!agreement.Selected) JavaScriptHelper.Click(Driver, agreement);
        ClickAndWaitForAjax(EditPriceSubmit, $"Seller: SAVE new price {price:0}");
    }

    private void ClickAndWaitForAjax(By by, string description)
    {
        var button = Elements.Clickable(by, description);
        Step(description, button);
        Elements.Click(button);
        WaitHelper.ForDocumentReady(Driver, Session.LongTimeout);
    }
}
