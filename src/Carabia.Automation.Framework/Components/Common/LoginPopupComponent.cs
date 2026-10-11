using Carabia.Automation.Framework.Base;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace Carabia.Automation.Framework.Components.Common;

/// <summary>
/// Client login popup (Views/Shared/_ModelLoginPartial.cshtml, #overlay). The framework only OPENS it,
/// optionally pre-selects the country code, and detects whether it is shown. It never types a phone
/// number, never requests an OTP and never submits it - authentication is manual by design.
/// </summary>
public sealed class LoginPopupComponent(BrowserSession session) : ComponentBase(session)
{
    private static readonly By SignInButton = By.Id("profileBtn");
    private static readonly By ActiveOverlay = By.CssSelector("#overlay.overlay--active");
    private static readonly By CountrySelect = By.CssSelector("#overlay .step--active .countrySelect");
    private static readonly By SelectedCountryLabel = By.CssSelector("#overlay .step--active .form-field__selected-code");
    private static readonly By PhoneInput = By.Id("phoneInput");
    private static readonly By ContinueButton = By.CssSelector("#overlay .step1Continue");

    public bool IsOpen => Elements.IsDisplayed(ActiveOverlay) && Elements.IsDisplayed(PhoneInput);

    public void WaitUntilOpen(string reason) =>
        Session.WaitFor($"the login popup to open ({reason})", _ => IsOpen);

    /// <summary>Clicks the header "Sign in" button.</summary>
    public void Open()
    {
        var button = Session.WaitFor("the Sign in button to be visible and enabled", d =>
            d.FindElements(SignInButton).FirstOrDefault(b => b.Displayed && b.Enabled));
        Step("Login: open Sign in", button);
        Elements.Click(button!);
        WaitUntilOpen("after clicking Sign in");
    }

    /// <summary>Used by the manual-authentication flow: open the popup and focus the phone field for the person.</summary>
    public void OpenForManualLogin(string? countryCode)
    {
        Open();
        if (!string.IsNullOrWhiteSpace(countryCode))
            TrySelectCountry(countryCode);
        var phone = Session.Visible(PhoneInput, "the phone-number field");
        phone.Click();
    }

    public bool TrySelectCountry(string countryCode)
    {
        try
        {
            var select = Session.WaitFor($"the country list to offer {countryCode}", d =>
            {
                var element = d.FindElement(CountrySelect);
                if (!element.Displayed || !element.Enabled) return null;
                var s = new SelectElement(element);
                return s.Options.Any(o => o.GetDomAttribute("value") == countryCode) ? s : null;
            });
            select!.SelectByValue(countryCode);
            return true;
        }
        catch (Exception exception) when (exception is Utilities.AutomationException or WebDriverException)
        {
            Session.Log($"Country {countryCode} could not be pre-selected; choose it yourself. ({exception.Message})");
            return false;
        }
    }

    public string SelectedCountryText => Driver.FindElement(SelectedCountryLabel).Text.Trim();

    public string SelectedCountryValue => new SelectElement(Driver.FindElement(CountrySelect)).SelectedOption.GetDomAttribute("value") ?? "";

    /// <summary>
    /// Types a NON-REAL number only to check client-side enablement of Continue; Continue is never clicked
    /// (same safe check as carabia-ui-tests LoginFormSmokeTests). No OTP is requested.
    /// </summary>
    public bool ContinueEnablesForSampleNumber(string sampleNumber)
    {
        var phone = Session.Visible(PhoneInput, "the phone-number field");
        phone.SendKeys(sampleNumber);
        return Utilities.WaitHelper.TryUntil(Driver,
            d => d.FindElement(ContinueButton).GetDomAttribute("aria-disabled") == "false", Timeout);
    }

    public bool ContinueIsDisabled => Driver.FindElement(ContinueButton).GetDomAttribute("aria-disabled") == "true";
}
