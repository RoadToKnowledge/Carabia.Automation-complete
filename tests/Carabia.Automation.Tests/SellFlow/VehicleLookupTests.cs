using Carabia.Automation.Framework.Flows;
using Carabia.Automation.Framework.Utilities;
using Carabia.Automation.Tests.Base;

namespace Carabia.Automation.Tests.SellFlow;

/// <summary>
/// Both vehicle paths up to the features step, as a visitor (ported from carabia-ui-tests SellCarLookupTests):
/// VIN decode and Year -> Make -> Model -> Spec -> Regional spec, then details, Google Places location,
/// owner notes, back/next navigation. Needs Autodata and Google Maps on DEV. Creates nothing.
/// </summary>
[TestFixture]
[Category(TestCategories.Public)]
[Category(TestCategories.SellFlow)]
[Category(TestCategories.Regression)]
[Parallelizable(ParallelScope.Self)]
public sealed class VehicleLookupTests : UiTestBase
{
    [Test]
    public void Vin_Lookup_Identifies_Vehicle_And_Details_Reach_Features()
    {
        var flow = new SellCarFlow(Session);
        flow.IdentifyVehicle(TestData.Vehicle, manual: false);
        flow.CompleteDetailsAndFeatures(TestData.Vehicle, RandomDataGenerator.AutomationMarker());
        Assert.That(flow.Page.Wizard.CurrentStep(), Is.EqualTo(3));
    }

    [Test]
    public void Manual_Selection_Identifies_Vehicle_And_Details_Reach_Features()
    {
        var flow = new SellCarFlow(Session);
        flow.IdentifyVehicle(TestData.Vehicle, manual: true);
        flow.CompleteDetailsAndFeatures(TestData.Vehicle, RandomDataGenerator.AutomationMarker());
        Assert.That(flow.Page.Wizard.CurrentStep(), Is.EqualTo(3));
    }

    [Test]
    public void Google_Places_Location_Resolves_City_Country_And_Coordinates()
    {
        var flow = new SellCarFlow(Session);
        flow.IdentifyVehicle(TestData.Vehicle, manual: false);
        var location = TestData.Vehicle.Location;
        flow.Page.Location.Select(location);
        Assert.Multiple(() =>
        {
            Assert.That(flow.Page.Location.City, Is.EqualTo(location.ExpectedCity));
            Assert.That(flow.Page.Location.CountryCode, Is.EqualTo(location.ExpectedCountryCode));
            Assert.That(flow.Page.Location.PlaceId, Is.Not.Empty);
            Assert.That(flow.Page.Location.Latitude, Is.Not.Empty);
            Assert.That(flow.Page.Location.Longitude, Is.Not.Empty);
        });
    }

    [Test]
    public void Back_Buttons_Return_To_Previous_Steps_And_Keep_The_Vehicle()
    {
        var flow = new SellCarFlow(Session);
        flow.IdentifyVehicle(TestData.Vehicle, manual: false);
        flow.CompleteDetailsAndFeatures(TestData.Vehicle, RandomDataGenerator.AutomationMarker());
        flow.Page.Wizard.Back(3);
        Assert.That(flow.Page.Wizard.Value("sellCarMileageInput").Replace(",", ""), Is.EqualTo(TestData.Vehicle.Mileage), "Mileage was lost going back.");
        flow.Page.Wizard.Back(2);
        Assert.That(flow.Page.Wizard.Value("sellCarVinInput"), Is.EqualTo(TestData.Vehicle.Vin), "The VIN was lost going back to step 1.");
    }

    [Test]
    public void Owner_Notes_Are_Saved_And_Shown_As_Chip()
    {
        var flow = new SellCarFlow(Session);
        flow.IdentifyVehicle(TestData.Vehicle, manual: false);
        var notes = flow.CompleteDetailsAndFeatures(TestData.Vehicle, RandomDataGenerator.AutomationMarker());
        Assert.Multiple(() =>
        {
            Assert.That(flow.Page.OwnerNotesChipText, Is.Not.Empty, "The notes chip is empty.");
            Assert.That(flow.Page.ReadSavedOwnerNotes(), Is.EqualTo(notes));
        });
    }

    [Test]
    public void Asking_Price_Step_Accepts_A_Price_And_Opens_Photos()
    {
        var flow = new SellCarFlow(Session);
        flow.IdentifyVehicle(TestData.Vehicle, manual: false);
        flow.CompleteDetailsAndFeatures(TestData.Vehicle, RandomDataGenerator.AutomationMarker());
        flow.Page.ContinueFromFeatures();
        flow.Page.Price.EnterPrice(TestData.Vehicle.AskingPrice);
        flow.Page.Price.Continue();
        Assert.Multiple(() =>
        {
            Assert.That(flow.Page.Wizard.CurrentStep(), Is.EqualTo(5));
            Assert.That(flow.Page.Photos.AcceptAttribute, Does.Contain(".jpg").And.Contain(".png").And.Contain(".webp"));
        });
    }
}
