using Carabia.Automation.Framework.Base;
using Carabia.Automation.Framework.Utilities;
using OpenQA.Selenium;

namespace Carabia.Automation.Framework.Components.Sell;

/// <summary>
/// Step handling of /personal/sell (Views/Personal/Sell.cshtml). The real step order from the source:
///   1 vehicle (VIN or Year/Make/Model/Spec/Regional spec)   #sell-car-step-1
///   2 vehicle details (mileage, colours, warranty, location) #sell-car-step-2
///   3 features / condition / owner notes                    #sell-car-step-3
///   4 asking price                                          #sell-car-step-4
///   5 photos (minimum 4)                                    #sell-car-step-5
///   6 ownership verification (Mulkiya, can be skipped)      #sell-car-step-6
///   7 listing plan (Premium / Premium+)                     #sell-car-step-7  (only when qualified)
///   8 boost                                                 #sell-car-step-8
///   9 summary + Submit Listing                              #sell-car-step-9
///  10 payment (only when a paid add-on was chosen)          #sell-car-step-10
///  11 "Listing submitted"                                   #sell-car-step-11
/// Back buttons: "#sell-car-step-N [data-step-back='N-1']".
/// </summary>
public sealed class SellWizardComponent(BrowserSession session) : ComponentBase(session)
{
    public const int LastStep = 11;
    private static readonly By Section = By.Id("sellCarSection");
    private static readonly By LookupMessage = By.Id("sellCarLookupMessage");

    public static By StepContainer(int step) => By.Id($"sell-car-step-{step}");

    public bool IsLoaded => Elements.Exists(Section) && Elements.Exists(StepContainer(1));

    public bool IsStepDisplayed(int step) => Driver.FindElements(StepContainer(step)).FirstOrDefault()?.Displayed == true;

    public int CurrentStep()
    {
        for (var step = 1; step <= LastStep; step++)
            if (IsStepDisplayed(step))
                return step;
        return 0;
    }

    /// <summary>Waits for a step to show and fails fast with the lookup error text if Autodata rejected the vehicle.</summary>
    public void WaitForStep(int step)
    {
        Session.WaitFor($"Sell My Car step {step}", d =>
        {
            var error = d.FindElements(LookupMessage).FirstOrDefault();
            if (error is not null && error.Displayed && !string.IsNullOrWhiteSpace(error.Text))
                throw new AutomationException($"Vehicle lookup failed: {error.Text.Trim()}");
            return IsStepDisplayed(step);
        }, Session.LongTimeout);
    }

    public string LookupMessageText => Driver.FindElements(LookupMessage).FirstOrDefault(e => e.Displayed)?.Text.Trim() ?? string.Empty;

    public IWebElement WaitForLookupMessage() => Session.Visible(LookupMessage, "the vehicle lookup / VIN validation message");

    public void Back(int fromStep)
    {
        var button = Driver.FindElement(By.CssSelector($"#sell-car-step-{fromStep} [data-step-back='{fromStep - 1}']"));
        Step($"Sell: Back from step {fromStep}", button);
        Elements.Click(button);
        WaitForStep(fromStep - 1);
    }

    public void ClickById(string id, string description)
    {
        var element = Session.Visible(By.Id(id), description);
        Step(description, element);
        Session.WaitFor($"{description} to be enabled", _ => element.Enabled);
        Elements.Click(element);
    }

    public void Click(IWebElement element, string description)
    {
        Step(description, element);
        Session.WaitFor($"{description} to be enabled", _ => element.Enabled);
        Elements.Click(element);
    }

    public IWebElement Field(string id) => Driver.FindElement(By.Id(id));
    public string Value(string id) => Field(id).GetDomProperty("value") ?? string.Empty;

    public void Fill(string id, string value, bool tabOut = true)
    {
        var input = Session.Visible(By.Id(id), id);
        Step($"Fill {id}: {value}", input);
        input.SendKeys(Keys.Control + "a");
        input.SendKeys(value);
        if (tabOut) input.SendKeys(Keys.Tab);
    }
}
