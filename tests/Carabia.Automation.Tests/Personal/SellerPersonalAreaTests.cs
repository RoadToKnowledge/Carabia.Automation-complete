using Carabia.Automation.Framework.Configuration;
using Carabia.Automation.Framework.Pages.Personal;
using Carabia.Automation.Framework.Utilities;
using Carabia.Automation.Tests.Base;

namespace Carabia.Automation.Tests.Personal;

/// <summary>
/// Signed-in Seller: My Garage > Selling (status groups, listing cards, Edit price / Remove listing dialogs).
/// Dialogs are opened and closed only; the guarded price edit runs only on the automation listing.
/// </summary>
[TestFixture]
[Category(TestCategories.Personal)]
[Category(TestCategories.Regression)]
public sealed class SellerPersonalAreaTests : SellerTestBase
{
    private MyGaragePage OpenSelling()
    {
        var garage = new MyGaragePage(Session);
        garage.OpenTab(MyGaragePage.GarageTab.Selling);
        return garage;
    }

    [Test]
    public void Selling_Tab_Shows_Status_Groups_With_Counts()
    {
        var garage = OpenSelling();
        var groups = garage.StatusGroups();
        Assert.Multiple(() =>
        {
            Assert.That(garage.ActiveTabText, Is.EqualTo("Selling"));
            Assert.That(groups.Select(g => g.Name), Does.Contain("Submissions").And.Contain("Live"));
            Assert.That(groups.All(g => g.Count >= 0), Is.True);
        });
    }

    [Test]
    public void Status_Group_Counts_Add_Up_To_The_Listing_Cards()
    {
        var garage = OpenSelling();
        var cards = garage.CardElements().Count;
        if (cards == 0)
            Assert.Inconclusive("The Seller test account has no listings yet. Submit one with SellerSellFlowTests (guarded).");
        // Every card is rendered inside exactly one status group, so the group counts must cover them all.
        Assert.That(garage.StatusGroups().Sum(g => g.Count), Is.GreaterThanOrEqualTo(cards));
    }

    [Test]
    public void Listing_Card_View_Listing_Links_Are_Listing_Routes()
    {
        var garage = OpenSelling();
        var hrefs = garage.ViewListingHrefs();
        if (hrefs.Count == 0) Assert.Inconclusive("No listings with a 'View Listing' link.");
        Assert.That(hrefs.All(h => UrlHelper.SellingIdFromUrl(h) is not null), Is.True, string.Join("\n", hrefs));
    }

    [Test]
    public void Edit_Price_Dialog_Opens_With_Price_Field_Without_Saving()
    {
        var garage = OpenSelling();
        var card = garage.CardElements().FirstOrDefault(garage.CardHasEditPrice);
        if (card is null) Assert.Inconclusive("No listing offers 'Edit price' (only active listings do).");
        garage.OpenEditPrice(card!);
        var dialog = new ListingManagementPage(Session);
        dialog.WaitForEditPriceDialog();
        Assert.That(dialog.EditPriceHasSumField, Is.True);
        dialog.CloseAnyDialog();
    }

    [Test]
    public void Remove_Listing_Dialog_Opens_With_Reasons_And_Can_Be_Cancelled()
    {
        var garage = OpenSelling();
        var card = garage.CardElements().FirstOrDefault(garage.CardHasRemoveListing);
        if (card is null) Assert.Inconclusive("No listing offers 'Remove listing'.");
        garage.OpenRemoveListing(card!);
        var dialog = new ListingManagementPage(Session);
        dialog.WaitForRemoveDialog();
        Assert.That(dialog.RemoveDialogHasReasons, Is.True);
        dialog.CancelRemoveDialog();
        Assert.That(dialog.IsRemoveDialogOpen, Is.False);
    }

    [Test]
    [Category(TestCategories.WritesData)]
    public void Seller_Can_Change_Price_Of_Automation_Listing()
    {
        RequireWritePermission(WriteKind.OfferWrite);
        var sellingId = TestData.Offers.AutomationSellingId
                        ?? throw new IgnoreException("Set Offers/OfferData.json AutomationSellingId (automation listing).");
        var garage = OpenSelling();
        var card = garage.CardForSelling(sellingId) ?? throw new AssertionException($"Listing {sellingId} is not in the Seller's garage.");
        if (!garage.CardHasEditPrice(card)) Assert.Inconclusive($"Listing {sellingId} is not active, so its price cannot be edited.");
        var newPrice = decimal.Parse(TestData.Vehicle.AskingPrice) + 1000;
        garage.OpenEditPrice(card);
        var dialog = new ListingManagementPage(Session);
        dialog.WaitForEditPriceDialog();
        dialog.SubmitNewPrice(newPrice);
        var details = new Framework.Pages.Public.ListingDetailsPage(Session);
        details.OpenListing(garage.ViewListingHrefs().First(h => UrlHelper.SellingIdFromUrl(h) == sellingId));
        Assert.That(details.Price, Is.EqualTo(newPrice));
    }
}
