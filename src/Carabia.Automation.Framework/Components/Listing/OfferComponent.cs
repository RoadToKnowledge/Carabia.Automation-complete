using Carabia.Automation.Framework.Base;
using OpenQA.Selenium;

namespace Carabia.Automation.Framework.Components.Listing;

/// <summary>
/// "Contact the Owner" actions on a listing (Selling.cshtml): .lps-contact-owner-btn opens
/// #contactOwnerModal (class "is-open"); options are ".lps-contact-owner-option" - "Make an Offer"
/// (.direct-payment, navigates to /Payment/PaymentDetails?sellingId=N) and "Request a Test Drive".
/// The button is only rendered for active listings that do not belong to the signed-in user (canInteract).
/// </summary>
public sealed class OfferComponent(BrowserSession session) : ComponentBase(session)
{
    private static readonly By ContactOwnerButtons = By.CssSelector(".lps-contact-owner-btn");
    private static readonly By Modal = By.Id("contactOwnerModal");
    private static readonly By ModalTitle = By.Id("contactOwnerTitle");
    private static readonly By Options = By.CssSelector("#contactOwnerModal .lps-contact-owner-option");
    private static readonly By MakeOfferOption = By.CssSelector("#contactOwnerModal .lps-contact-owner-option.direct-payment");
    private static readonly By CloseButton = By.CssSelector("#contactOwnerModal .lps-contact-owner-close");
    private static readonly By TestDriveOverlay = By.Id("modalTDOverlay");

    public bool HasContactOwnerButton => Driver.FindElements(ContactOwnerButtons).Any(b => b.Displayed);

    public bool IsModalOpen =>
        Driver.FindElements(Modal).FirstOrDefault() is { } modal && (modal.GetDomAttribute("class") ?? string.Empty).Contains("is-open");

    public void OpenContactOwner()
    {
        var button = Session.WaitFor("a visible 'Contact the Owner' button", d => d.FindElements(ContactOwnerButtons).FirstOrDefault(b => b.Displayed));
        Step("Listing: click Contact the Owner", button);
        Elements.Click(button!);
        Session.WaitFor("the Contact the Owner dialog to open", _ => IsModalOpen);
    }

    public string ModalTitleText => Driver.FindElement(ModalTitle).Text.Trim();

    public IReadOnlyList<string> OptionTitles() =>
        Driver.FindElements(Options).Where(o => o.Displayed)
            .Select(o => o.FindElements(By.CssSelector(".lps-contact-owner-copy strong")).FirstOrDefault()?.Text.Trim() ?? o.Text.Trim())
            .ToList();

    public void CloseWithButton()
    {
        var close = Elements.Clickable(CloseButton, "the Contact the Owner close button");
        Elements.Click(close);
        Session.WaitFor("the Contact the Owner dialog to close", _ => !IsModalOpen);
    }

    public void CloseWithEscape()
    {
        new OpenQA.Selenium.Interactions.Actions(Driver).SendKeys(Keys.Escape).Perform();
        Session.WaitFor("the Contact the Owner dialog to close with Escape", _ => !IsModalOpen);
    }

    /// <summary>Navigates to the offer page. Does not submit anything.</summary>
    public void ChooseMakeAnOffer()
    {
        var option = Elements.Clickable(MakeOfferOption, "the 'Make an Offer' option");
        Step("Listing: choose Make an Offer", option);
        Elements.Click(option);
        Session.WaitFor("the offer page /Payment/PaymentDetails to open",
            d => d.Url.Contains("/payment/paymentdetails", StringComparison.OrdinalIgnoreCase), Session.LongTimeout);
    }

    /// <summary>Opens the test-drive request sheet. The request itself is not submitted here.</summary>
    public void ChooseRequestTestDrive()
    {
        var option = Session.WaitFor("the 'Request a Test Drive' option", d =>
            d.FindElements(Options).FirstOrDefault(o => o.Displayed && o.Text.Contains("Test Drive", StringComparison.OrdinalIgnoreCase)));
        Step("Listing: choose Request a Test Drive", option);
        Elements.Click(option!);
    }

    /// <summary>test-drive.js adds "active" to #modalTDOverlay (window.openTestDriveModal).</summary>
    public bool IsTestDriveSheetOpen =>
        Driver.FindElements(TestDriveOverlay).FirstOrDefault() is { } overlay && (overlay.GetDomAttribute("class") ?? string.Empty).Contains("active");

    public void CloseTestDriveSheet()
    {
        ((IJavaScriptExecutor)Driver).ExecuteScript("window.closeTestDriveModal && window.closeTestDriveModal();");
        Session.WaitFor("the test-drive sheet to close", _ => !IsTestDriveSheetOpen);
    }
}
