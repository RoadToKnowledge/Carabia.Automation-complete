using Carabia.Automation.Framework.Base;
using OpenQA.Selenium;

namespace Carabia.Automation.Framework.Components.Common;

/// <summary>
/// Mobile side drawer (_HeaderPartial.cshtml + drawer script in _Layout.cshtml):
/// #crbOpenDrawerBtn adds "crb-active-state" to #crbMobileDrawer and #crbDrawerOverlay;
/// #crbCloseDrawerBtn and the overlay remove it. Menu sections use .cs-dropdown / .cs-dropdown-toggle.
/// </summary>
public sealed class MobileNavigationComponent(BrowserSession session) : ComponentBase(session)
{
    private const string ActiveClass = "crb-active-state";
    private static readonly By OpenButton = By.Id("crbOpenDrawerBtn");
    private static readonly By CloseButton = By.Id("crbCloseDrawerBtn");
    private static readonly By Drawer = By.Id("crbMobileDrawer");
    private static readonly By Overlay = By.Id("crbDrawerOverlay");
    private static readonly By DrawerSellMyCar = By.CssSelector("#crbMobileDrawer > a.crb-btn-sell-car");
    private static readonly By DrawerSections = By.CssSelector("#crbMobileDrawer .cs-section-title");
    private static readonly By DrawerDropdowns = By.CssSelector("#crbMobileDrawer .cs-dropdown");
    private static readonly By DrawerLinks = By.CssSelector("#crbMobileDrawer a[href]");

    public bool HasOpenButton => Elements.IsDisplayed(OpenButton);

    public bool IsOpen => HasClass(Drawer, ActiveClass);
    public bool IsOverlayActive => HasClass(Overlay, ActiveClass);

    public void Open()
    {
        var button = Elements.Clickable(OpenButton, "the mobile menu button (#crbOpenDrawerBtn)");
        Step("Mobile: open menu drawer", button);
        Elements.Click(button);
        Session.WaitFor("the mobile drawer to open (crb-active-state)", _ => IsOpen);
    }

    public void CloseWithButton()
    {
        var button = Elements.Clickable(CloseButton, "the drawer close button (#crbCloseDrawerBtn)");
        Step("Mobile: close drawer with X", button);
        Elements.Click(button);
        Session.WaitFor("the mobile drawer to close", _ => !IsOpen);
    }

    public void CloseWithOverlay()
    {
        Step("Mobile: close drawer by clicking the overlay");
        // The overlay sits beside the drawer; click its exposed right-hand area.
        ((IJavaScriptExecutor)Driver).ExecuteScript("document.getElementById('crbDrawerOverlay').click();");
        Session.WaitFor("the mobile drawer to close after clicking the overlay", _ => !IsOpen);
    }

    public bool HasSellMyCar => Elements.IsDisplayed(DrawerSellMyCar);

    public IReadOnlyList<string> SectionTitles() =>
        Driver.FindElements(DrawerSections).Select(e => e.Text.Trim()).Where(t => t.Length > 0).ToList();

    public int DropdownCount => Driver.FindElements(DrawerDropdowns).Count;

    /// <summary>Toggles the n-th drawer dropdown (".cs-dropdown") and reports whether it is expanded (.cs-active).</summary>
    public bool ToggleDropdown(int index)
    {
        var dropdown = Driver.FindElements(DrawerDropdowns)[index];
        var toggle = dropdown.FindElement(By.CssSelector(".cs-dropdown-toggle"));
        Step($"Mobile: toggle drawer section '{toggle.Text.Trim()}'", toggle);
        Elements.Click(toggle);
        return (dropdown.GetDomAttribute("class") ?? string.Empty).Contains("cs-active");
    }

    public IReadOnlyList<string> LinkHrefs() =>
        Driver.FindElements(DrawerLinks).Select(a => a.GetDomProperty("href") ?? string.Empty).Where(h => h.Length > 0).ToList();

    private bool HasClass(By by, string css) =>
        Driver.FindElements(by).FirstOrDefault() is { } element && (element.GetDomAttribute("class") ?? string.Empty).Split(' ').Contains(css);
}
