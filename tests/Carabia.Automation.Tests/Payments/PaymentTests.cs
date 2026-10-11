using Carabia.Automation.Framework.Configuration;
using Carabia.Automation.Framework.Flows;
using Carabia.Automation.Framework.Pages.Offers;
using Carabia.Automation.Framework.Pages.Payment;
using Carabia.Automation.Framework.Utilities;
using Carabia.Automation.Tests.Base;

namespace Carabia.Automation.Tests.Payments;

/// <summary>
/// Payment safety on the offer page (visitor). The buyer offer flow currently submits offers without a card
/// (submitZeroPaymentOffer -> POST /Payment/ConfirmPayment with paymentIntentId = null); the Stripe card screen (#page4)
/// is not reachable from the offer flow.
/// </summary>
[TestFixture]
[Category(TestCategories.Public)]
[Category(TestCategories.Payments)]
[Category(TestCategories.Regression)]
[Parallelizable(ParallelScope.Self)]
public sealed class PaymentPageSafetyTests : UiTestBase
{
    [Test]
    public void Offer_Page_Starts_On_Offer_Screen_And_Hides_Card_Payment()
    {
        var sellingId = new BrowseCarsFlow(Session).FirstActiveSellingId();
        var page = new MakeOfferPage(Session);
        page.OpenForSelling(sellingId);
        page.WaitUntilLoaded();
        Assert.Multiple(() =>
        {
            Assert.That(page.ActiveScreenId, Is.EqualTo("page1"));
            Assert.That(page.IsCardPaymentScreenShown, Is.False, "The card payment screen must not be shown up front.");
        });
    }

    [Test]
    public async Task Payment_Write_Endpoints_Reject_Anonymous_Calls()
    {
        using var http = NewHttpProbe();
        // Anonymous, empty requests must not succeed (they need a signed-in client and a valid JSON body).
        var confirm = await http.PostFormAsync("/Payment/ConfirmPayment", new Dictionary<string, string>());
        var intent = await http.PostFormAsync("/Payment/CreateSellListingPaymentIntent", new Dictionary<string, string>());
        Assert.Multiple(() =>
        {
            Assert.That(confirm.Body, Does.Not.Contain("\"success\":true"), confirm.ToString());
            Assert.That(intent.Body, Does.Not.Contain("\"success\":true"), intent.ToString());
        });
    }
}

/// <summary>
/// Sell-listing payment (paid boost) as the Seller. Reaching the payment step writes nothing; authorising a card
/// does. Card tests run only with AllowPaymentWriteTests + AllowSellSubmissionTests on DEV AND a pk_test_ Stripe key.
/// </summary>
[TestFixture]
[Category(TestCategories.Payments)]
[Category(TestCategories.SellFlow)]
[Category(TestCategories.Regression)]
public sealed class SellListingPaymentTests : SellerTestBase
{
    private const string PaidBoost = "1 Week Boost";

    private StripePaymentPage OpenPaymentStep()
    {
        var flow = new SellCarFlow(Session);
        flow.IdentifyVehicle(TestData.Vehicle, manual: false);
        flow.CompleteDetailsAndFeatures(TestData.Vehicle, RandomDataGenerator.AutomationMarker());
        flow.CompleteToBoostStep(TestData.Vehicle, SellCarFlow.PhotoFiles(Settings));
        flow.Page.ChooseBoostAndContinue(PaidBoost);
        flow.Page.OpenPaymentStepFromSummary();
        return new StripePaymentPage(Session);
    }

    [Test]
    public void Paid_Boost_Opens_Payment_Step_With_Total_And_Card_Field_Without_Charging()
    {
        var payment = OpenPaymentStep();
        Assert.Multiple(() =>
        {
            Assert.That(payment.IsDisplayed, Is.True);
            Assert.That(payment.Total, Is.GreaterThan(0));
            Assert.That(payment.CardElementMounted, Is.True, "The Stripe card element did not mount in #card-element.");
            Assert.That(payment.PayButtonEnabled, Is.False, "Pay must stay disabled until card details are complete.");
        });
        TestContext.Out.WriteLine($"Stripe mode: {payment.Mode}. No card data was entered.");
    }

    [Test]
    [Category(TestCategories.WritesData)]
    public void Declined_Test_Card_Shows_Error_And_Stays_On_Payment()
    {
        RequireWritePermission(WriteKind.PaymentWrite);
        RequireWritePermission(WriteKind.SellSubmission);
        var payment = OpenPaymentStep();
        if (payment.Mode != "test") Assert.Ignore($"Stripe key mode is '{payment.Mode}'; card tests need a pk_test_ key.");
        payment.EnterTestCard(TestData.Payments.DeclinedCard);
        Session.WaitFor("Pay to become enabled", _ => payment.PayButtonEnabled);
        payment.Pay();
        Session.WaitFor("a card error", _ => payment.CardErrorText.Length > 0, Session.LongTimeout);
        Assert.That(payment.IsDisplayed, Is.True, "A declined card must keep the seller on the payment step.");
    }

    [Test]
    [Category(TestCategories.WritesData)]
    public void Successful_Test_Card_Authorises_And_Submits_Listing()
    {
        RequireWritePermission(WriteKind.PaymentWrite);
        RequireWritePermission(WriteKind.SellSubmission);
        var payment = OpenPaymentStep();
        if (payment.Mode != "test") Assert.Ignore($"Stripe key mode is '{payment.Mode}'; card tests need a pk_test_ key.");
        payment.EnterTestCard(TestData.Payments.SuccessCard);
        Session.WaitFor("Pay to become enabled", _ => payment.PayButtonEnabled);
        payment.Pay();
        var wizard = new Framework.Components.Sell.SellWizardComponent(Session);
        Session.WaitFor("the 'Listing submitted' screen after authorisation", _ => wizard.IsStepDisplayed(11), TimeSpan.FromMinutes(3));
    }
}
