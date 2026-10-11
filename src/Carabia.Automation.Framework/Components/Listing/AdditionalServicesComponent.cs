using Carabia.Automation.Framework.Base;
using OpenQA.Selenium;

namespace Carabia.Automation.Framework.Components.Listing;

/// <summary>
/// Additional services grid #lpsAdditionalServicesTrack (.lps-service-card / .lps-service-title).
/// "Request Inspection" (data-action="inspection") always carries "require-login": auth-popup.js asks
/// /Identity/AuthStatus and opens #lps-as-modal for signed-in clients or the login popup for visitors. Sending (#sendLpsInspectionRequest) creates a
/// service request, so it is only used by guarded write tests.
/// </summary>
public sealed class AdditionalServicesComponent(BrowserSession session) : ComponentBase(session)
{
    public static readonly string[] ExpectedServices =
        ["Request Inspection", "Arrange Shipping", "Registration Service", "Finance Options", "Extend Warranty", "Insurance Quote"];

    private static readonly By Track = By.Id("lpsAdditionalServicesTrack");
    private static readonly By Cards = By.CssSelector("#lpsAdditionalServicesTrack .lps-service-card");
    private static readonly By InspectionCard = By.CssSelector("#lpsAdditionalServicesTrack .lps-service-card[data-action='inspection']");
    private static readonly By InspectionModal = By.Id("lps-as-modal");
    private static readonly By InspectionModalClose = By.CssSelector("#lps-as-modal .lps-as-close-btn");
    private static readonly By SendInspection = By.Id("sendLpsInspectionRequest");
    private static readonly By InspectionMessage = By.Id("lpsInspectionRequestMessage");

    public bool IsPresent => Elements.Exists(Track);

    public IReadOnlyList<string> ServiceTitles() =>
        Driver.FindElements(Cards).Select(c => c.FindElements(By.CssSelector(".lps-service-title")).FirstOrDefault()?.Text.Trim() ?? string.Empty).ToList();

    public IReadOnlyList<string> DisabledServiceTitles() =>
        Driver.FindElements(Cards)
            .Where(c => c.FindElements(By.CssSelector(".lps-service-status-disabled")).Count > 0)
            .Select(c => c.FindElements(By.CssSelector(".lps-service-title")).FirstOrDefault()?.Text.Trim() ?? string.Empty)
            .ToList();

    public bool InspectionRequiresLogin =>
        (Driver.FindElement(InspectionCard).GetDomAttribute("class") ?? string.Empty).Contains("require-login");

    public void ClickInspection()
    {
        var card = Driver.FindElement(InspectionCard);
        Step("Services: click Request Inspection", card);
        Elements.Click(card);
    }

    public bool IsInspectionModalOpen => Elements.IsDisplayed(InspectionModal);

    public void CloseInspectionModal()
    {
        Elements.Click(Elements.Clickable(InspectionModalClose, "the inspection dialog close button"));
        Session.WaitFor("the inspection dialog to close", _ => !IsInspectionModalOpen);
    }

    /// <summary>WRITE: creates an inspection service request. Callers must check SafetyGuard first.</summary>
    public string SendInspectionRequest()
    {
        var send = Elements.Clickable(SendInspection, "the Send inspection request button");
        Elements.Click(send);
        return Session.WaitFor("the inspection request result", d =>
        {
            var button = d.FindElement(SendInspection);
            var message = d.FindElement(InspectionMessage);
            if (button.Text.Contains("Request sent", StringComparison.OrdinalIgnoreCase)) return "Request sent";
            return message.Displayed && message.Text.Trim().Length > 0 ? message.Text.Trim() : null;
        }, Session.LongTimeout)!;
    }
}
