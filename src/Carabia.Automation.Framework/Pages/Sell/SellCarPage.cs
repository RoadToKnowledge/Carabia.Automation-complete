using Carabia.Automation.Framework.Base;
using Carabia.Automation.Framework.Components.Sell;
using Carabia.Automation.Framework.Models.Results;
using Carabia.Automation.Framework.Models.TestData;
using Carabia.Automation.Framework.Utilities;
using OpenQA.Selenium;

namespace Carabia.Automation.Framework.Pages.Sell;

/// <summary>
/// PersonalController.Sell (GET is [AllowAnonymous]) -> Views/Personal/Sell.cshtml, route /personal/sell.
/// Steps 1-3 work for visitors; listing qualification (step 6 -> 7, POST /Personal/GetSellCarListingQualification)
/// and submission require a signed-in client. Logic ported from carabia-ui-tests SellCarPage.
/// </summary>
public sealed class SellCarPage(BrowserSession session) : PageBase(session)
{
    private static readonly By OwnerNotesButton = By.CssSelector("button[onclick='openOtherNotesPopup()']");
    private static readonly By OwnerNotesSave = By.CssSelector("#other-notes-overlay .btn-save");
    private static readonly By OwnerNotesBack = By.CssSelector("#other-notes-overlay .btn-back");
    private static readonly By FeaturesNext = By.CssSelector("#sell-car-step-3 [data-sell-car-target-step='4']");
    private static readonly By InteriorTab = By.CssSelector(".tab[onclick*='interior']");
    private static readonly By PremiumPlan = By.CssSelector("input[name='pricing_plan'][value='Premium']");
    private static readonly By NoBoostPlan = By.CssSelector("input[name='boost_plan'][data-no-boost='true']");
    private static readonly By SuccessTitle = By.CssSelector("#sell-car-step-11 .success-title");

    protected override string Path => "/personal/sell";

    public SellWizardComponent Wizard { get; } = new(session);
    public VehicleSelectorComponent Vehicle { get; } = new(session);
    public LocationSelectorComponent Location { get; } = new(session);
    public PriceSelectorComponent Price { get; } = new(session);
    public ImageUploadComponent Photos { get; } = new(session);

    public override void Open()
    {
        base.Open();
        Wizard.WaitForStep(1);
        Step("Sell My Car: step 1 (vehicle)");
    }

    // ---------- Step 2: vehicle details ----------

    public void FillVehicleDetails(VehicleTestData vehicle)
    {
        Wizard.Fill("sellCarMileageInput", vehicle.Mileage);
        SelectColor("exterior", vehicle.ExteriorColor);
        SelectColor("interior", vehicle.InteriorColor);
        Vehicle.Choose("sellCarWarrantyInput", vehicle.Warranty);
        Location.Select(vehicle.Location);
    }

    public void ContinueFromDetails()
    {
        Wizard.ClickById("sellCarVehicleDetailsNext", "Sell: vehicle details Next");
        Wizard.WaitForStep(3);
    }

    public bool DetailsNextEnabled => Wizard.Field("sellCarVehicleDetailsNext").Enabled;

    // ---------- Step 3: features, condition, owner notes ----------

    public void AddOwnerNotes(string notes)
    {
        Wizard.Click(Driver.FindElement(OwnerNotesButton), "Sell: add owner notes");
        Wizard.Fill("otherNotesInput", notes, tabOut: false);
        Wizard.Click(Driver.FindElement(OwnerNotesSave), "Sell: save owner notes");
        Session.WaitFor("the owner notes popup to close", _ => !Wizard.Field("otherNotesInput").Displayed);
    }

    /// <summary>The chip truncates long notes, so the popup is reopened to read the saved text.</summary>
    public string ReadSavedOwnerNotes()
    {
        Wizard.Click(Driver.FindElement(OwnerNotesButton), "Sell: reopen owner notes");
        var value = Wizard.Value("otherNotesInput");
        Wizard.Click(Driver.FindElement(OwnerNotesBack), "Sell: close owner notes");
        return value;
    }

    public string OwnerNotesChipText => Wizard.Field("other-note-wrap").Text.Trim();

    public void ContinueFromFeatures()
    {
        Wizard.Click(Driver.FindElement(FeaturesNext), "Sell: continue to asking price");
        Wizard.WaitForStep(4);
    }

    // ---------- Steps 6-8: ownership, plan, boost ----------

    public void SkipOwnershipVerification()
    {
        Wizard.ClickById("sellCarOwnerVerificationSkip", "Sell: skip ownership verification");
        Session.WaitFor("listing qualification to show plans (step 7) or boosts (step 8); requires a signed-in client",
            _ => Wizard.IsStepDisplayed(7) || Wizard.IsStepDisplayed(8), Session.LongTimeout);
    }

    /// <summary>Chooses the free options: Premium without paid upgrade (if offered) and "No Boost".</summary>
    public void ChooseFreePlanAndNoBoost()
    {
        if (Wizard.IsStepDisplayed(7))
        {
            var premium = Driver.FindElement(PremiumPlan);
            if (!premium.Selected)
                Wizard.Click(premium.FindElement(By.XPath("ancestor::label[1]")), "Sell: select Premium (no paid upgrade)");
            Wizard.ClickById("sellCarPremiumContinueBtn", "Sell: continue from plans");
        }

        Wizard.WaitForStep(8);
        var noBoost = Driver.FindElement(NoBoostPlan);
        if (!noBoost.Selected)
            Wizard.Click(noBoost.FindElement(By.XPath("ancestor::label[1]")), "Sell: select No Boost");
        Wizard.ClickById("sellCarBoostContinueBtn", "Sell: continue to summary");
        Wizard.WaitForStep(9);
    }

    /// <summary>Chooses a boost by its value ("No Boost", "1 Week Boost", "1 Month Boost") on step 8 and continues to the summary.</summary>
    public void ChooseBoostAndContinue(string boostValue)
    {
        Wizard.WaitForStep(8);
        var input = Driver.FindElement(By.CssSelector($"input[name='boost_plan'][value='{boostValue}']"));
        if (!input.Selected)
            Wizard.Click(input.FindElement(By.XPath("ancestor::label[1]")), $"Sell: select {boostValue}");
        Wizard.ClickById("sellCarBoostContinueBtn", "Sell: continue to summary");
        Wizard.WaitForStep(9);
    }

    /// <summary>Goes from step 6 to step 8 choosing the free Premium plan when step 7 is shown.</summary>
    public void SkipToBoostStep()
    {
        if (Wizard.IsStepDisplayed(7))
        {
            var premium = Driver.FindElement(PremiumPlan);
            if (!premium.Selected)
                Wizard.Click(premium.FindElement(By.XPath("ancestor::label[1]")), "Sell: select Premium (no paid upgrade)");
            Wizard.ClickById("sellCarPremiumContinueBtn", "Sell: continue from plans");
        }
        Wizard.WaitForStep(8);
    }

    /// <summary>With a paid add-on, "Submit" on the summary only opens the payment step (goToSellCarStep(10)); nothing is written.</summary>
    public void OpenPaymentStepFromSummary()
    {
        var total = TextParser.Digits(SummaryText("summaryPaymentTotal"));
        if (total <= 0)
            throw new AutomationException("Opening the payment step needs a paid add-on; with total 0 the button would SUBMIT the listing.");
        Wizard.ClickById("sellCarSummarySubmitBtn", "Sell: continue to payment (paid add-on selected)");
        Wizard.WaitForStep(10);
    }

    // ---------- Step 9: summary ----------

    public string SummaryText(string id) => Wizard.Field(id).Text.Trim();

    public SellListingResult ReadSummary(string marker) => new()
    {
        Marker = marker,
        SummaryCarName = SummaryText("summaryCarName"),
        SummaryMeta = SummaryText("summaryCarMeta"),
        SummaryAskingPrice = TextParser.Digits(SummaryText("summaryAskingPrice")),
        SummaryColours = SummaryText("summaryColours"),
        SummaryLocation = SummaryText("summaryLocation"),
        SummaryOwnerNotes = SummaryText("summaryOwnerNotes"),
        SummaryPhotoCount = SummaryText("summaryPhotoCount"),
        SummaryPaymentTotal = TextParser.Digits(SummaryText("summaryPaymentTotal"))
    };

    public string SummaryCoverSource => Wizard.Field("summaryCoverImage").GetDomProperty("src") ?? string.Empty;
    public bool SummarySubmitVisible => Wizard.Field("sellCarSummarySubmitBtn").Displayed;

    /// <summary>
    /// WRITE: submits the listing (POST /Personal/Sell). Only allowed when the payment total is 0, so this can
    /// never reach the Stripe payment step. Callers must check SafetyGuard (AllowSellSubmissionTests) first.
    /// </summary>
    public string SubmitFreeListing()
    {
        var total = TextParser.Digits(SummaryText("summaryPaymentTotal"));
        if (total != 0)
            throw new AutomationException($"Refusing to submit: the summary total is {total}, which would open the payment step.");
        Wizard.ClickById("sellCarSummarySubmitBtn", "Sell: SUBMIT LISTING (automation test listing)");
        Session.WaitFor("the 'Listing submitted' screen (step 11)", _ => Wizard.IsStepDisplayed(11), TimeSpan.FromMinutes(3));
        return Driver.FindElement(SuccessTitle).Text.Trim();
    }

    private void SelectColor(string group, string color)
    {
        if (group == "interior")
            Wizard.Click(Driver.FindElement(InteriorTab), "Sell: open interior colours");
        var item = Driver.FindElements(By.CssSelector($"#{group}-grid .color-item"))
                       .SingleOrDefault(e => e.FindElement(By.CssSelector(".color-label")).Text.Trim().Equals(color, StringComparison.OrdinalIgnoreCase))
                   ?? throw new AutomationException($"Expected {group} colour '{color}' to be offered.");
        Wizard.Click(item, $"Sell: select {color} ({group})");
    }
}
