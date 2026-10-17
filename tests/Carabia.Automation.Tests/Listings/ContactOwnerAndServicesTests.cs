using Carabia.Automation.Framework.Components.Listing;
using Carabia.Automation.Framework.Flows;
using Carabia.Automation.Framework.Pages.Offers;
using Carabia.Automation.Framework.Pages.Public;
using Carabia.Automation.Tests.Base;

namespace Carabia.Automation.Tests.Listings;

/// <summary>"Contact the Owner" dialog (Make an Offer / Request a Test Drive) and the additional services grid, as a visitor.</summary>
[TestFixture]
[Category(TestCategories.Public)]
[Category(TestCategories.Listings)]
[Category(TestCategories.Offers)]
[Category(TestCategories.Regression)]
[Parallelizable(ParallelScope.Self)]
public sealed class ContactOwnerAndServicesTests : UiTestBase
{
    private ListingDetailsPage OpenListing()
    {
        new BrowseCarsFlow(Session).OpenListingFromResults();
        return new ListingDetailsPage(Session);
    }

    [Test]
    public void Contact_The_Owner_Opens_Dialog_With_Offer_And_Test_Drive_Options()
    {
        var page = OpenListing();
        Assert.That(page.ContactOwner.HasContactOwnerButton, Is.True, "Active listings show 'Contact the Owner' to visitors.");
        page.ContactOwner.OpenContactOwner();
        Assert.Multiple(() =>
        {
            Assert.That(page.ContactOwner.ModalTitleText, Is.EqualTo("Contact the Owner"));
            Assert.That(page.ContactOwner.OptionTitles(), Does.Contain("Make an Offer"));
            Assert.That(page.ContactOwner.OptionTitles().Any(t => t.Contains("Test Drive")), Is.True);
        });
        page.ContactOwner.CloseWithButton();
    }

    [Test]
    public void Contact_Dialog_Closes_With_Escape()
    {
        var page = OpenListing();
        page.ContactOwner.OpenContactOwner();
        page.ContactOwner.CloseWithEscape();
        Assert.That(page.ContactOwner.IsModalOpen, Is.False);
    }

    [Test]
    public void Make_An_Offer_Opens_The_Offer_Page_For_This_Listing()
    {
        var page = OpenListing();
        var sellingId = page.SellingId!.Value;
        page.ContactOwner.OpenContactOwner();
        page.ContactOwner.ChooseMakeAnOffer();
        var offer = new MakeOfferPage(Session);
        offer.WaitUntilLoaded();
        Assert.Multiple(() =>
        {
            Assert.That(Session.Driver.Url, Does.Contain($"sellingId={sellingId}"));
            Assert.That(offer.HeadingText, Is.EqualTo("Make an offer"));
        });
    }

    [Test]
    public void Visitor_Requesting_A_Test_Drive_Is_Asked_To_Sign_In()
    {
        var page = OpenListing();
        page.ContactOwner.OpenContactOwner();
        page.ContactOwner.ChooseRequestTestDrive();
        page.LoginPopup.WaitUntilOpen("a visitor chose Request a Test Drive");
        Assert.That(page.ContactOwner.IsTestDriveSheetOpen, Is.False);
    }

    [Test]
    public void Additional_Services_Grid_Lists_All_Services()
    {
        var page = OpenListing();
        Assert.That(page.Services.IsPresent, Is.True, "#lpsAdditionalServicesTrack is missing.");
        Assert.That(page.Services.ServiceTitles(), Is.EquivalentTo(AdditionalServicesComponent.ExpectedServices));
    }

    [Test]
    public void Visitor_Requesting_Inspection_Is_Asked_To_Sign_In()
    {
        var page = OpenListing();
        Assert.That(page.Services.InspectionRequiresLogin, Is.True, "For visitors the inspection card carries 'require-login'.");
        page.Services.ClickInspection();
        page.LoginPopup.WaitUntilOpen("a visitor clicked Request Inspection");
        Assert.That(page.Services.IsInspectionModalOpen, Is.False, "Visitors must not reach the inspection request dialog.");
    }
}

/// <summary>Signed-in Buyer on a listing: test-drive sheet and inspection dialog open (nothing is submitted).</summary>
[TestFixture]
[Category(TestCategories.Listings)]
[Category(TestCategories.Regression)]
public sealed class BuyerListingInteractionTests : BuyerTestBase
{
    private ListingDetailsPage OpenListing()
    {
        new BrowseCarsFlow(Session).OpenListingFromResults();
        return new ListingDetailsPage(Session);
    }

    [Test]
    public void Buyer_Can_Open_And_Close_The_Test_Drive_Request_Sheet()
    {
        var page = OpenListing();
        page.ContactOwner.OpenContactOwner();
        page.ContactOwner.ChooseRequestTestDrive();
        Session.WaitFor("the test-drive sheet (#modalTDOverlay.active)", _ => page.ContactOwner.IsTestDriveSheetOpen);
        page.ContactOwner.CloseTestDriveSheet();
        // The request itself (POST /Sellings/PostTestDriveRequest) is not sent: it notifies a real seller.
    }

    [Test]
    public void Buyer_Can_Open_And_Close_The_Inspection_Request_Dialog()
    {
        var page = OpenListing();
        // The card keeps "require-login"; auth-popup.js sees the signed-in client and opens the dialog instead of the login popup.
        page.Services.ClickInspection();
        Session.WaitFor("the inspection dialog (#lps-as-modal)", _ => page.Services.IsInspectionModalOpen);
        Assert.Multiple(() =>
        {
            Assert.That(page.Services.IsInspectionModalOpen, Is.True, "Inspection dialog did not open.");
            Assert.That(page.Services.HasInspectionFormFields, Is.True, "Inspection dialog must have form input fields.");
        });
        page.Services.CloseInspectionModal();
        // "Send" (POST /System/AddInspectionReportRequest) is not clicked: it creates a service request.
    }
}
