using Carabia.Automation.Framework.Components.Sell;
using Carabia.Automation.Framework.Configuration;
using Carabia.Automation.Framework.Flows;
using Carabia.Automation.Framework.Pages.Personal;
using Carabia.Automation.Framework.Utilities;
using Carabia.Automation.Tests.Base;

namespace Carabia.Automation.Tests.SellFlow;

/// <summary>
/// Signed-in Seller: photos, ownership verification, plans, boost and the summary (carabia-ui-tests SellCarSummaryTests),
/// plus invalid upload and - only when AllowSellSubmissionTests=true on DEV - the real submission and its
/// visibility in My Garage.
/// </summary>
[TestFixture]
[Category(TestCategories.SellFlow)]
[Category(TestCategories.Regression)]
public sealed class SellerSellFlowTests : SellerTestBase
{
    private string[] Photos => SellCarFlow.PhotoFiles(Settings);

    private SellCarFlow StartToPhotos(string marker, bool manual = false)
    {
        var flow = new SellCarFlow(Session);
        flow.IdentifyVehicle(TestData.Vehicle, manual);
        flow.CompleteDetailsAndFeatures(TestData.Vehicle, marker);
        flow.Page.ContinueFromFeatures();
        flow.Page.Price.EnterPrice(TestData.Vehicle.AskingPrice);
        flow.Page.Price.Continue();
        return flow;
    }

    [Test]
    public void Photo_Step_Requires_At_Least_Four_Photos()
    {
        var flow = StartToPhotos(RandomDataGenerator.AutomationMarker());
        flow.Page.Photos.ClickNext();
        var error = flow.Page.Photos.WaitForError();
        Assert.Multiple(() =>
        {
            Assert.That(error, Does.Contain(ImageUploadComponent.MinimumPhotosMessage));
            Assert.That(flow.Page.Wizard.CurrentStep(), Is.EqualTo(5));
        });
        flow.Page.Photos.Upload(Photos.Take(ImageUploadComponent.MinimumPhotos - 1).ToArray());
        flow.Page.Photos.WaitForLoadedPreviews(ImageUploadComponent.MinimumPhotos - 1);
        flow.Page.Photos.ClickNext();
        Assert.That(flow.Page.Wizard.CurrentStep(), Is.EqualTo(5), "Three photos must not be enough.");
    }

    [Test]
    public void Uploaded_Photos_Show_Previews_And_First_Is_Cover()
    {
        var flow = StartToPhotos(RandomDataGenerator.AutomationMarker());
        flow.Page.Photos.Upload(Photos);
        flow.Page.Photos.WaitForLoadedPreviews(Photos.Length);
        Assert.Multiple(() =>
        {
            Assert.That(flow.Page.Photos.CellCount, Is.EqualTo(Photos.Length));
            Assert.That(flow.Page.Photos.CoverSource, Is.Not.Empty, "No cover photo marked (.pug-is-cover).");
            Assert.That(flow.Page.Photos.ErrorVisible, Is.False);
        });
    }

    /// <summary>
    /// The input's accept attribute only filters the OS dialog; pugHandleUpload() itself does not check file types
    /// (Sell.cshtml). This test documents the expected behaviour and fails while a .txt file is accepted as a photo.
    /// </summary>
    [Test]
    [Category("KnownIssue")]
    public void Non_Image_File_Is_Not_Accepted_As_A_Photo()
    {
        var flow = StartToPhotos(RandomDataGenerator.AutomationMarker());
        flow.Page.Photos.Upload([TestDataReader.FilePath(Settings, "Uploads", "invalid-file.txt")]);
        WaitHelper.TryUntil(Session.Driver, _ => flow.Page.Photos.CellCount > 0, TimeSpan.FromSeconds(5));
        Assert.That(flow.Page.Photos.CellCount, Is.EqualTo(0),
            "A .txt file was added to the photo grid. pugHandleUpload should reject non-image files (see docs/KNOWN_LIMITATIONS.md).");
    }

    [TestCase(false, TestName = "Vin_Flow_Reaches_Summary_With_Correct_Values_Without_Submitting")]
    [TestCase(true, TestName = "Manual_Flow_Reaches_Summary_With_Correct_Values_Without_Submitting")]
    public void Flow_Reaches_Summary_With_Correct_Values_Without_Submitting(bool manual)
    {
        var marker = RandomDataGenerator.AutomationMarker();
        var vehicle = TestData.Vehicle;
        var flow = new SellCarFlow(Session);
        flow.IdentifyVehicle(vehicle, manual);
        var notes = flow.CompleteDetailsAndFeatures(vehicle, marker);
        flow.CompleteToSummary(vehicle, Photos);
        var summary = flow.ReadSummary(marker);
        Assert.Multiple(() =>
        {
            Assert.That(summary.SummaryCarName, Does.Contain(vehicle.Make).IgnoreCase.And.Contain(vehicle.Model).IgnoreCase);
            Assert.That(summary.SummaryMeta, Does.Contain(vehicle.Year).And.Contain(vehicle.Trim).IgnoreCase);
            Assert.That(summary.SummaryMeta.Replace(",", ""), Does.Contain(vehicle.Mileage));
            Assert.That(summary.SummaryAskingPrice, Is.EqualTo(decimal.Parse(vehicle.AskingPrice)));
            Assert.That(summary.SummaryColours, Is.EqualTo($"{vehicle.ExteriorColor}, {vehicle.InteriorColor}"));
            Assert.That(flow.Page.SummaryText("summaryVehicleWarranty"), Is.EqualTo(vehicle.Warranty));
            Assert.That(summary.SummaryLocation, Does.Contain(vehicle.Location.ExpectedCity).IgnoreCase);
            Assert.That(summary.SummaryOwnerNotes, Is.EqualTo(notes));
            Assert.That(summary.SummaryPhotoCount, Is.EqualTo($"{Photos.Length} photos uploaded"));
            Assert.That(flow.Page.SummaryText("summaryOwnerVerificationText"), Is.EqualTo("Mulkiya not uploaded"));
            Assert.That(summary.SummaryPaymentTotal, Is.EqualTo(0m), "Free plan + No Boost must cost nothing.");
            Assert.That(flow.Page.SummarySubmitVisible, Is.True);
            Assert.That(flow.Page.Wizard.IsStepDisplayed(10), Is.False, "The test must stop before payment.");
        });
        // Deliberately no click on Submit Listing here.
    }

    [Test]
    public void Summary_Back_Returns_To_Boost_Step()
    {
        var flow = new SellCarFlow(Session);
        flow.IdentifyVehicle(TestData.Vehicle, manual: false);
        flow.CompleteDetailsAndFeatures(TestData.Vehicle, RandomDataGenerator.AutomationMarker());
        flow.CompleteToSummary(TestData.Vehicle, Photos);
        flow.Page.Wizard.Back(9);
        Assert.That(flow.Page.Wizard.CurrentStep(), Is.EqualTo(8));
    }

    [Test]
    [Category(TestCategories.WritesData)]
    public void Seller_Can_Submit_A_Free_Listing_And_See_It_In_Submissions()
    {
        RequireWritePermission(WriteKind.SellSubmission);
        var garage = new MyGaragePage(Session);
        garage.OpenTab(MyGaragePage.GarageTab.Selling);
        var submissionsBefore = garage.StatusGroups().FirstOrDefault(g => g.Name == "Submissions").Count;

        var marker = RandomDataGenerator.AutomationMarker();
        var flow = new SellCarFlow(Session);
        flow.IdentifyVehicle(TestData.Vehicle, manual: false);
        flow.CompleteDetailsAndFeatures(TestData.Vehicle, marker);
        flow.CompleteToSummary(TestData.Vehicle, Photos);
        var title = flow.Page.SubmitFreeListing();
        Assert.That(title, Is.EqualTo("Listing submitted"));
        TestContext.Out.WriteLine($"Automation listing submitted with owner-notes marker {marker}. It is in moderation; " +
                                  "an admin can find it in Administrator > Sellings > Moderation.");

        garage.OpenTab(MyGaragePage.GarageTab.Selling);
        var submissionsAfter = garage.StatusGroups().FirstOrDefault(g => g.Name == "Submissions").Count;
        Assert.That(submissionsAfter, Is.EqualTo(submissionsBefore + 1), "The new listing should appear under Submissions (moderation).");
    }
}
