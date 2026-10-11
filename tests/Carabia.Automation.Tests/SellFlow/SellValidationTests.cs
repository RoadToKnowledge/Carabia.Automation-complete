using Carabia.Automation.Framework.Components.Sell;
using Carabia.Automation.Framework.Pages.Sell;
using Carabia.Automation.Tests.Base;
using OpenQA.Selenium.Support.UI;

namespace Carabia.Automation.Tests.SellFlow;

/// <summary>
/// Step-1 rules of /personal/sell for visitors (ported from carabia-ui-tests SellCarValidationTests, extended):
/// VIN/manual exclusivity, Next-button states, dependent dropdowns, VIN length validation, VIN help.
/// No listing is created.
/// </summary>
[TestFixture]
[Category(TestCategories.Public)]
[Category(TestCategories.SellFlow)]
[Category(TestCategories.Regression)]
[Parallelizable(ParallelScope.Self)]
public sealed class SellValidationTests : UiTestBase
{
    private SellCarPage Open()
    {
        var page = new SellCarPage(Session);
        page.Open();
        return page;
    }

    [Test]
    public void Empty_Vehicle_Step_Cannot_Continue()
    {
        var page = Open();
        var v = page.Vehicle;
        Assert.Multiple(() =>
        {
            Assert.That(v.IsEnabled(VehicleSelectorComponent.VinNext), Is.False, "VIN Next must be disabled when the VIN is empty.");
            Assert.That(v.IsEnabled(VehicleSelectorComponent.ManualNext), Is.False, "Manual Next must be disabled until the vehicle is complete.");
            Assert.That(v.IsEnabled(VehicleSelectorComponent.YearInput), Is.True, "Year must be selectable without a VIN.");
            Assert.That(v.YearOptionCount, Is.GreaterThan(1), "Years come from AutodataModelYears; none were rendered.");
            Assert.That(page.Wizard.IsStepDisplayed(2), Is.False);
        });
    }

    [TestCaseSource(typeof(TestData), nameof(TestData.InvalidVinCases))]
    public void Vin_With_Wrong_Length_Is_Rejected_On_Step_One(string vin)
    {
        var page = Open();
        page.Vehicle.TypeVin(vin);
        Session.WaitFor("VIN Next to become enabled", _ => page.Vehicle.IsEnabled(VehicleSelectorComponent.VinNext));
        page.Vehicle.SubmitVin();
        var error = page.Wizard.WaitForLookupMessage();
        Assert.Multiple(() =>
        {
            Assert.That(error.Text, Does.Contain("VIN must be 17 characters"));
            Assert.That(page.Wizard.IsStepDisplayed(1), Is.True);
            Assert.That(page.Wizard.IsStepDisplayed(2), Is.False);
        });
    }

    [Test]
    public void Typing_A_Vin_Disables_Manual_Selection_And_Clearing_It_Restores_It()
    {
        var page = Open();
        page.Vehicle.TypeVin("ABC123");
        Assert.That(page.Vehicle.IsEnabled(VehicleSelectorComponent.YearInput), Is.False, "A VIN must disable manual Year.");
        page.Vehicle.ClearVin();
        Session.WaitFor("manual Year to be enabled again", _ => page.Vehicle.IsEnabled(VehicleSelectorComponent.YearInput));
        Assert.That(page.Vehicle.IsEnabled(VehicleSelectorComponent.VinNext), Is.False, "VIN Next must be disabled again.");
    }

    [Test]
    public void Vin_Takes_Priority_Over_A_Started_Manual_Selection()
    {
        // syncSellCarVehicleInputs(): a non-empty VIN disables Year, hides the manual fields and the manual Next.
        var page = Open();
        page.Vehicle.Choose(VehicleSelectorComponent.YearInput, TestData.Vehicle.Year);
        Session.WaitFor("the Make dropdown to appear after choosing a year", _ => page.Vehicle.Field(VehicleSelectorComponent.MakeInput).Displayed);
        Assert.That(page.Vehicle.IsEnabled(VehicleSelectorComponent.VinNext), Is.False, "VIN Next must stay disabled without a VIN.");
        page.Vehicle.TypeVin(TestData.Vehicle.Vin);
        Session.WaitFor("manual fields to hide once a VIN is typed", _ => !page.Vehicle.Field(VehicleSelectorComponent.MakeInput).Displayed);
        Assert.Multiple(() =>
        {
            Assert.That(page.Vehicle.IsEnabled(VehicleSelectorComponent.YearInput), Is.False, "Year must be disabled while a VIN is entered.");
            Assert.That(page.Vehicle.IsEnabled(VehicleSelectorComponent.VinNext), Is.True, "VIN Next must be enabled with a VIN.");
            Assert.That(page.Vehicle.IsEnabled(VehicleSelectorComponent.ManualNext), Is.False, "Manual Next must be disabled while a VIN is entered.");
        });
    }

    [Test]
    public void Dependent_Dropdowns_Load_In_Order_And_Next_Waits_For_Regional_Spec()
    {
        var page = Open();
        var vehicle = TestData.Vehicle;
        page.Vehicle.Choose(VehicleSelectorComponent.YearInput, vehicle.Year);
        Assert.That(page.Vehicle.WaitForOptions(VehicleSelectorComponent.MakeInput), Does.Contain(vehicle.Make), "Makes for the year (GET /api/autodata/sell-your-car/makes).");
        page.Vehicle.Choose(VehicleSelectorComponent.MakeInput, vehicle.Make);
        Assert.That(page.Vehicle.WaitForOptions(VehicleSelectorComponent.ModelInput), Does.Contain(vehicle.Model), "Models for the make.");
        page.Vehicle.Choose(VehicleSelectorComponent.ModelInput, vehicle.Model);
        Assert.That(page.Vehicle.WaitForOptions(VehicleSelectorComponent.SpecInput), Does.Contain(vehicle.Trim), "Specifications for the model.");
        page.Vehicle.Choose(VehicleSelectorComponent.SpecInput, vehicle.Trim);
        Assert.That(page.Vehicle.IsEnabled(VehicleSelectorComponent.ManualNext), Is.False,
            "Manual Next must stay disabled until the regional specification is chosen.");
        page.Vehicle.Choose(VehicleSelectorComponent.RegionalSpecInput, vehicle.RegionalSpec);
        Session.WaitFor("manual Next to become enabled", _ => page.Vehicle.IsEnabled(VehicleSelectorComponent.ManualNext));
    }

    [Test]
    public void Changing_Year_Resets_The_Dependent_Selections()
    {
        var page = Open();
        var vehicle = TestData.Vehicle;
        page.Vehicle.Choose(VehicleSelectorComponent.YearInput, vehicle.Year);
        page.Vehicle.Choose(VehicleSelectorComponent.MakeInput, vehicle.Make);
        var otherYear = new SelectElement(page.Vehicle.Field(VehicleSelectorComponent.YearInput)).Options
            .Select(o => o.Text.Trim()).First(t => t.Length == 4 && t != vehicle.Year);
        page.Vehicle.Choose(VehicleSelectorComponent.YearInput, otherYear);
        Session.WaitFor("the model selection to be cleared", _ =>
            new SelectElement(page.Vehicle.Field(VehicleSelectorComponent.ModelInput)).SelectedOption.GetDomAttribute("value") is null or "");
        Assert.That(page.Vehicle.IsEnabled(VehicleSelectorComponent.ManualNext), Is.False);
    }

    [Test]
    public void Vin_Help_Expands_And_Collapses()
    {
        var page = Open();
        Assert.That(page.Vehicle.VinHelpVisible, Is.False, "VIN help should start collapsed.");
        page.Vehicle.ToggleVinHelp();
        Session.WaitFor("VIN help to open", _ => page.Vehicle.VinHelpVisible);
        Assert.That(page.Vehicle.VinHelpText, Does.Contain("17-character VIN"));
        page.Vehicle.ToggleVinHelp();
        Session.WaitFor("VIN help to close", _ => !page.Vehicle.VinHelpVisible);
    }
}
