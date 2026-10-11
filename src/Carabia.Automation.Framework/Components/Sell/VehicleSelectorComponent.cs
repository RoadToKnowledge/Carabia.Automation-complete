using Carabia.Automation.Framework.Base;
using Carabia.Automation.Framework.Models.TestData;
using Carabia.Automation.Framework.Utilities;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace Carabia.Automation.Framework.Components.Sell;

/// <summary>
/// Step 1 of the sell wizard. VIN path: #sellCarVinInput -> #sellCarVinNext (POST /api/autodata/sell-your-car/decode-vin).
/// Manual path: #sellCarYearInput -> #sellCarMakeInput -> #sellCarModelInput -> #sellCarSpecInput -> #sellCarRegionalSpecInput
/// (GET /api/autodata/sell-your-car/makes|models|specifications) -> #sellCarManualNext.
/// Typing a VIN disables the manual Year; clearing it re-enables it.
/// </summary>
public sealed class VehicleSelectorComponent(BrowserSession session) : ComponentBase(session)
{
    public const string VinInput = "sellCarVinInput";
    public const string VinNext = "sellCarVinNext";
    public const string YearInput = "sellCarYearInput";
    public const string MakeInput = "sellCarMakeInput";
    public const string ModelInput = "sellCarModelInput";
    public const string SpecInput = "sellCarSpecInput";
    public const string RegionalSpecInput = "sellCarRegionalSpecInput";
    public const string ManualNext = "sellCarManualNext";

    private static readonly By VinHelpToggle = By.CssSelector(".van-find-text");
    private static readonly By VinHelpAnswer = By.CssSelector(".van-find-text-ans");
    private static readonly By VehicleCardHeader = By.CssSelector("#sell-car-step-2 .vs-card-sec .vs-card-header");

    private SellWizardComponent Wizard { get; } = new(session);

    public IWebElement Field(string id) => Driver.FindElement(By.Id(id));
    public bool IsEnabled(string id) => Field(id).Enabled;
    public int YearOptionCount => new SelectElement(Field(YearInput)).Options.Count;

    public void TypeVin(string vin)
    {
        var input = Field(VinInput);
        Step($"Sell: type VIN {vin}", input);
        input.SendKeys(vin);
    }

    public void ClearVin()
    {
        var input = Field(VinInput);
        input.SendKeys(Keys.Control + "a");
        input.SendKeys(Keys.Backspace);
    }

    public void SubmitVin() => Wizard.ClickById(VinNext, "Sell: VIN Next");

    public void LookupByVin(string vin)
    {
        Wizard.Fill(VinInput, vin);
        SubmitVin();
        Wizard.WaitForStep(2);
    }

    /// <summary>Selects one dependent dropdown option by text once it has loaded (Autodata calls can take time).</summary>
    public void Choose(string id, string text)
    {
        var option = Session.WaitFor($"'{text}' in {id}; check the Autodata dropdown response if it never appears", _ =>
        {
            var select = Field(id);
            return select.Displayed && select.Enabled
                ? new SelectElement(select).Options.FirstOrDefault(o => o.Text.Trim().Equals(text, StringComparison.OrdinalIgnoreCase))
                : null;
        }, Session.LongTimeout);
        Step($"Sell: select {text}", Field(id));
        new SelectElement(Field(id)).SelectByValue(option!.GetDomAttribute("value")!);
    }

    public void SelectManually(VehicleTestData vehicle, bool stopBeforeRegionalSpec = false)
    {
        Choose(YearInput, vehicle.Year);
        Choose(MakeInput, vehicle.Make);
        Choose(ModelInput, vehicle.Model);
        Choose(SpecInput, vehicle.Trim);
        if (!stopBeforeRegionalSpec)
            Choose(RegionalSpecInput, vehicle.RegionalSpec);
    }

    public void SubmitManual()
    {
        Wizard.ClickById(ManualNext, "Sell: manual Next");
        Wizard.WaitForStep(2);
    }

    /// <summary>Waits for a dependent dropdown to be filled; returns its option texts.</summary>
    public IReadOnlyList<string> WaitForOptions(string id) =>
        WaitHelper.ForSelectOptions(Driver, By.Id(id), $"{id} options to load", Session.LongTimeout)
            .Options.Select(o => o.Text.Trim()).ToList();

    public string VehicleCardHeaderText => Session.Visible(VehicleCardHeader, "the identified vehicle card on step 2").Text;

    public bool VinHelpVisible => Driver.FindElement(VinHelpAnswer).Displayed;
    public string VinHelpText => Driver.FindElement(VinHelpAnswer).Text;

    public void ToggleVinHelp()
    {
        var toggle = Driver.FindElement(VinHelpToggle);
        Step("Sell: toggle 'where is my VIN' help", toggle);
        toggle.Click();
    }
}
