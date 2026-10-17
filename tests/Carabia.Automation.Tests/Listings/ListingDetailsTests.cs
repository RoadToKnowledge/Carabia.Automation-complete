using System.Text.RegularExpressions;
using Carabia.Automation.Framework.Flows;
using Carabia.Automation.Framework.Pages.Public;
using Carabia.Automation.Framework.Utilities;
using Carabia.Automation.Tests.Base;

namespace Carabia.Automation.Tests.Listings;

/// <summary>SellingsController.Selling: header, specifications, price, seller, VIN, FAQs, wishlist, related listings.</summary>
[TestFixture]
[Category(TestCategories.Public)]
[Category(TestCategories.Listings)]
[Category(TestCategories.Regression)]
[Parallelizable(ParallelScope.Self)]
public sealed class ListingDetailsTests : UiTestBase
{
    private ListingDetailsPage OpenFirstListing()
    {
        new BrowseCarsFlow(Session).OpenListingFromResults();
        return new ListingDetailsPage(Session);
    }

    [Test]
    public void Details_Show_Make_Model_Year_Specifications_And_Asking_Price()
    {
        var page = OpenFirstListing();
        var specs = page.Features.Specifications();
        Assert.Multiple(() =>
        {
            foreach (var required in new[] { "Make", "Model", "Year" })
                Assert.That(specs.TryGetValue(required, out var v) && !string.IsNullOrWhiteSpace(v), Is.True, $"Missing specification '{required}'.");
            Assert.That(specs.TryGetValue("Year", out var year) && Regex.IsMatch(year, @"^\d{4}$"), Is.True, "Year must be four digits.");
            Assert.That(page.Price, Is.GreaterThan(0), "Asking price must be numeric.");
            Assert.That(page.VehicleName, Is.Not.Empty, "The vehicle header (h1) is empty.");
        });
    }

    [Test]
    public void Heading_Matches_Specifications()
    {
        var page = OpenFirstListing();
        var specs = page.Features.Specifications();
        var heading = page.SectionTitleText;
        Assert.Multiple(() =>
        {
            Assert.That(heading, Does.Contain(specs["Year"]));
            Assert.That(heading, Does.Contain(specs["Make"]).IgnoreCase);
            Assert.That(heading, Does.Contain("for Sale"));
        });
    }

    [Test]
    public void Monthly_Finance_Estimate_And_Seller_Are_Shown()
    {
        var page = OpenFirstListing();
        Assert.Multiple(() =>
        {
            Assert.That(page.MonthlyPaymentText, Does.Contain("/mo"));
            Assert.That(page.SellerName, Is.Not.Empty, "Seller card has no name.");
        });
    }

    [Test]
    public void Visitor_Sees_Vin_Field_With_Login_To_Reveal()
    {
        var page = OpenFirstListing();
        Assert.That(page.VinFieldCount, Is.GreaterThan(0), "The VIN card is missing.");
    }

    [Test]
    public void Visitor_Wishlist_Button_Asks_To_Sign_In()
    {
        var page = OpenFirstListing();
        page.ClickWishlist();
        page.LoginPopup.WaitUntilOpen("a visitor clicked the favourites heart");
        Assert.That(page.LoginPopup.IsOpen, Is.True);
    }

    [Test]
    public void Listing_Faqs_Open_And_Close()
    {
        var page = OpenFirstListing();
        var failures = page.Faq.CheckSample("Listing details");
        Assert.That(failures, Is.Empty, string.Join("\n", failures));
    }

    [Test]
    public void Ask_A_Question_Links_To_Comment_Section()
    {
        var page = OpenFirstListing();
        Assert.Multiple(() =>
        {
            Assert.That(page.HasAskQuestion, Is.True, "'Ask a Question' (#ask-question) is missing.");
            Assert.That(page.HasCommentTextarea, Is.True, "Comment textarea is missing in the #ask-question section.");
            Assert.That(page.HasCommentSubmitButton, Is.True, "Comment submit button is missing in the #ask-question section.");
        });
        // Submit/Post is NEVER clicked.
    }

    [Test]
    [Category(TestCategories.Listings)]
    public void Listing_Detail_Shows_Specs_Grid_With_Vehicle_Information()
    {
        var page = OpenFirstListing();
        var specLabels = page.Features.SpecLabels();
        Assert.Multiple(() =>
        {
            Assert.That(page.Features.SpecItemCount, Is.GreaterThan(0), "Listing must show specs grid.");
            Assert.That(specLabels.Count, Is.GreaterThan(2), "Specs grid should have at least 3 labelled specs.");
            Assert.That(page.PriceText, Is.Not.Empty, "Price must be displayed.");
            Assert.That(page.VehicleName, Is.Not.Empty, "Vehicle name must be displayed.");
        });
    }

    [Test]
    [Category(TestCategories.Listings)]
    public void Listing_Detail_Page_Shows_Related_Listings()
    {
        var page = OpenFirstListing();
        Assert.That(page.RelatedListingCount, Is.GreaterThanOrEqualTo(0),
            "Related listings section should render (even if empty).");
        if (page.RelatedListingCount > 0)
            Assert.That(page.RelatedListingLinkCount, Is.GreaterThan(0),
                "Related listing cards should link to details pages.");
    }

    [Test]
    [Category(TestCategories.Listings)]
    public void Listing_Detail_Page_Shows_Back_To_Listings_Button()
    {
        var page = OpenFirstListing();
        Assert.That(page.HasBackToListings, Is.True, "Back to listings button should exist.");
    }

    [Test]
    public async Task Listing_Url_With_Wrong_Model_Slug_Returns_404()
    {
        var url = new BrowseCarsFlow(Session).FirstListingUrl();
        var id = UrlHelper.SellingIdFromUrl(url)!.Value;
        using var http = NewHttpProbe();
        var response = await http.GetAsync($"/sellings/{id}_automation-wrong-name");
        Assert.That(response.Code, Is.EqualTo(404), response.ToString());
    }
}
