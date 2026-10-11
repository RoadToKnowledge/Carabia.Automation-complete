using Carabia.Automation.Framework.Configuration;
using Carabia.Automation.Framework.Flows;
using Carabia.Automation.Framework.Pages.Offers;
using Carabia.Automation.Framework.Pages.Personal;
using Carabia.Automation.Framework.Pages.Public;
using Carabia.Automation.Tests.Base;

namespace Carabia.Automation.Tests.Offers;

/// <summary>
/// Offer negotiation between two test accounts on ONE automation-owned listing (OfferData.AutomationSellingId).
/// Buyer: own-listing guard, valid offer -> "Offer submitted". Seller: sees offer, counter, decline, accept.
/// All writes need Execution.AllowOfferWriteTests=true on DEV and a configured automation listing; otherwise
/// they are reported as Ignored with the reason. Run order inside the fixture is fixed with [Order].
/// </summary>
[TestFixture]
[Category(TestCategories.Offers)]
[Category(TestCategories.WritesData)]
[Category(TestCategories.Regression)]
public sealed class BuyerOfferTests : BuyerTestBase
{
    private int AutomationSellingId()
    {
        RequireWritePermission(WriteKind.OfferWrite);
        return TestData.Offers.AutomationSellingId
               ?? throw new IgnoreException("Set Offers/OfferData.json AutomationSellingId to a listing created by the Seller test account.");
    }

    [Test, Order(1)]
    public void Buyer_Sees_Contact_The_Owner_On_Another_Sellers_Listing()
    {
        new BrowseCarsFlow(Session).OpenListingFromResults();
        var details = new ListingDetailsPage(Session);
        // canInteract = active && not own listing; the Buyer account must not own the first listing.
        Assert.That(details.ContactOwner.HasContactOwnerButton, Is.True);
    }

    [Test, Order(2)]
    public void Buyer_Can_Submit_An_Offer_On_The_Automation_Listing()
    {
        var sellingId = AutomationSellingId();
        var result = new OfferFlow(Session).BuyerSubmitsOffer(sellingId, TestData.Offers.ValidOfferRatio);
        Assert.That(result.Message, Is.EqualTo("Offer submitted"));
        TestContext.Out.WriteLine($"Offer of {result.Amount:0} submitted on listing {sellingId}.");
    }

    [Test, Order(3)]
    public void Buyer_Sees_The_Listing_Under_Buying_After_Offering()
    {
        var sellingId = AutomationSellingId();
        var garage = new MyGaragePage(Session);
        garage.OpenTab(MyGaragePage.GarageTab.Buying);
        Assert.That(garage.CardForSelling(sellingId), Is.Not.Null, "Listings with an active offer by the Buyer appear under Buying.");
    }

    [Test, Order(4)]
    public void Second_Offer_Below_Minimum_Is_Rejected()
    {
        var sellingId = AutomationSellingId();
        var page = new MakeOfferPage(Session);
        page.OpenForSelling(sellingId);
        page.WaitUntilLoaded();
        page.EnterOffer(page.AskingPriceValue * TestData.Offers.TooLowOfferRatio);
        Assert.That(page.WaitForValidationMessage(), Does.Contain("too low"));
    }
}

[TestFixture]
[Category(TestCategories.Offers)]
[Category(TestCategories.WritesData)]
[Category(TestCategories.Regression)]
public sealed class SellerOfferResponseTests : SellerTestBase
{
    private int AutomationSellingId()
    {
        RequireWritePermission(WriteKind.OfferWrite);
        // Accepting may capture a payment authorisation (PersonalController.AcceptOffer), so payment writes must be allowed too.
        return TestData.Offers.AutomationSellingId
               ?? throw new IgnoreException("Set Offers/OfferData.json AutomationSellingId to a listing created by the Seller test account.");
    }

    [Test, Order(1)]
    public void Seller_Cannot_Make_An_Offer_On_Own_Listing()
    {
        var sellingId = AutomationSellingId();
        var garage = new MyGaragePage(Session);
        garage.OpenTab(MyGaragePage.GarageTab.Selling);
        var card = garage.CardForSelling(sellingId);
        Assert.That(card, Is.Not.Null, $"Listing {sellingId} must belong to the Seller test account.");
        var href = garage.ViewListingHrefs().First(h => Framework.Utilities.UrlHelper.SellingIdFromUrl(h) == sellingId);
        var details = new ListingDetailsPage(Session);
        details.OpenListing(href);
        Assert.That(details.ContactOwner.HasContactOwnerButton, Is.False, "Owners must not see 'Contact the Owner' on their own listing.");
    }

    [Test, Order(2)]
    public void Seller_Sees_Received_Offer_With_Buyer_And_Amount()
    {
        var dialog = new OfferFlow(Session).SellerOpensOffer(AutomationSellingId());
        Assert.Multiple(() =>
        {
            Assert.That(dialog.OfferBuyerText, Is.Not.Empty);
            Assert.That(dialog.OfferSumValue, Is.GreaterThan(0));
        });
        dialog.CloseAnyDialog();
    }

    [Test, Order(3)]
    public void Seller_Can_Counter_Offer()
    {
        var sellingId = AutomationSellingId();
        var dialog = new OfferFlow(Session).SellerOpensOffer(sellingId);
        var counter = decimal.Round(dialog.OfferSumValue / TestData.Offers.ValidOfferRatio * TestData.Offers.CounterOfferRatio);
        dialog.CounterOffer(counter);
        var garage = new MyGaragePage(Session);
        garage.OpenTab(MyGaragePage.GarageTab.Selling);
        Assert.That(garage.CardForSelling(sellingId), Is.Not.Null);
    }

    [Test, Order(4)]
    public void Seller_Can_Decline_An_Offer()
    {
        var sellingId = AutomationSellingId();
        var garage = new MyGaragePage(Session);
        garage.OpenTab(MyGaragePage.GarageTab.Selling);
        var card = garage.CardForSelling(sellingId)!;
        if (!garage.CardHasManageOffer(card))
            Assert.Inconclusive("No active buyer offer to decline (the Buyer tests create one).");
        garage.OpenManageOffer(card);
        var dialog = new ListingManagementPage(Session);
        dialog.WaitForOfferDialog();
        dialog.DeclineOffer();
        garage.OpenTab(MyGaragePage.GarageTab.Selling);
        Assert.That(garage.CardHasManageOffer(garage.CardForSelling(sellingId)!), Is.False, "The declined offer should no longer be manageable.");
    }

    [Test, Order(5)]
    public void Seller_Can_Accept_An_Offer()
    {
        var sellingId = AutomationSellingId();
        RequireWritePermission(WriteKind.PaymentWrite);
        var garage = new MyGaragePage(Session);
        garage.OpenTab(MyGaragePage.GarageTab.Selling);
        var card = garage.CardForSelling(sellingId)!;
        if (!garage.CardHasManageOffer(card))
            Assert.Inconclusive("No active buyer offer to accept. Run BuyerOfferTests first.");
        garage.OpenManageOffer(card);
        var dialog = new ListingManagementPage(Session);
        dialog.WaitForOfferDialog();
        dialog.AcceptOffer();
        garage.OpenTab(MyGaragePage.GarageTab.Selling);
        Assert.That(garage.CardHasManageOffer(garage.CardForSelling(sellingId)!), Is.False, "After accepting, the offer is no longer pending.");
    }
}
