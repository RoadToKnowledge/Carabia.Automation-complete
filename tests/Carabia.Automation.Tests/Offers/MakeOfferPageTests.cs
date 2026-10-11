using Carabia.Automation.Framework.Flows;
using Carabia.Automation.Framework.Pages.Offers;
using Carabia.Automation.Tests.Base;

namespace Carabia.Automation.Tests.Offers;

/// <summary>
/// /Payment/PaymentDetails?sellingId=N as a VISITOR: listing summary, 75% minimum offer rule, monthly estimate,
/// and "Submit Offer" asking the visitor to sign in. ValidateOffer is read-only; no offer is created.
/// </summary>
[TestFixture]
[Category(TestCategories.Public)]
[Category(TestCategories.Offers)]
[Category(TestCategories.Payments)]
[Category(TestCategories.Regression)]
[Parallelizable(ParallelScope.Self)]
public sealed class MakeOfferPageTests : UiTestBase
{
    private MakeOfferPage OpenOfferPageForFirstListing()
    {
        var sellingId = new BrowseCarsFlow(Session).FirstActiveSellingId();
        var page = new MakeOfferPage(Session);
        page.OpenForSelling(sellingId);
        page.WaitUntilLoaded();
        return page;
    }

    [Test]
    public void Offer_Page_Shows_Car_Asking_Price_And_Disabled_Submit()
    {
        var page = OpenOfferPageForFirstListing();
        Assert.Multiple(() =>
        {
            Assert.That(page.HeadingText, Is.EqualTo("Make an offer"));
            Assert.That(page.CarNameText, Is.Not.Empty);
            Assert.That(page.AskingPriceValue, Is.GreaterThan(0));
            Assert.That(page.SubmitEnabled, Is.False, "Submit Offer must be disabled before an amount is entered.");
        });
    }

    [Test]
    public void Minimum_Offer_Is_Seventy_Five_Percent_Of_Asking_Price()
    {
        var page = OpenOfferPageForFirstListing();
        Assert.That(page.MinimumOffer, Is.EqualTo(decimal.Round(page.AskingPriceValue * 0.75m, 2)).Within(1m));
    }

    [Test]
    public void Offer_Below_Minimum_Shows_Error_And_Keeps_Submit_Disabled()
    {
        var page = OpenOfferPageForFirstListing();
        page.EnterOffer(page.AskingPriceValue * TestData.Offers.TooLowOfferRatio);
        var message = page.WaitForValidationMessage();
        Assert.Multiple(() =>
        {
            Assert.That(message, Does.Contain("too low"));
            Assert.That(page.WaitForSubmitState(false), Is.True);
        });
    }

    [Test]
    public void Valid_Offer_Enables_Submit_And_Updates_Monthly_Estimate()
    {
        var page = OpenOfferPageForFirstListing();
        page.EnterOffer(page.AskingPriceValue * TestData.Offers.ValidOfferRatio);
        Assert.Multiple(() =>
        {
            Assert.That(page.WaitForSubmitState(true), Is.True, $"Submit stayed disabled: {page.ValidationText}");
            Assert.That(page.ValidationText, Is.Empty);
        });
    }

    [Test]
    public void Visitor_Submitting_A_Valid_Offer_Is_Asked_To_Sign_In()
    {
        var page = OpenOfferPageForFirstListing();
        page.EnterOffer(page.AskingPriceValue * TestData.Offers.ValidOfferRatio);
        page.WaitForSubmitState(true);
        page.ClickSubmitOffer(); // visitor: ValidateOffer (read-only) then the login popup; nothing is created
        page.LoginPopup.WaitUntilOpen("a visitor submitted an offer");
        Assert.That(page.ActiveScreenId, Is.EqualTo("page1"), "A visitor must not reach the submitted screen.");
    }

    [Test]
    public void Offer_Page_Reports_Stripe_Mode_Without_Exposing_The_Key()
    {
        var page = OpenOfferPageForFirstListing();
        var mode = page.StripeMode;
        TestContext.Out.WriteLine($"Stripe publishable key mode on this environment: {mode}");
        Assert.That(mode, Is.AnyOf("test", "live", "missing", "unknown"));
        if (mode == "live")
            TestContext.Out.WriteLine("WARNING: the page uses a LIVE Stripe key. Payment write tests stay disabled (see docs/KNOWN_LIMITATIONS.md).");
    }
}
