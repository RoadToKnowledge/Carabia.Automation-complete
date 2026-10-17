using Carabia.Automation.Framework.Components.Common;
using Carabia.Automation.Framework.Pages.Public;
using Carabia.Automation.Framework.Utilities;
using Carabia.Automation.Tests.Base;
using Carabia.Automation.Framework.Pages.Admin;

namespace Carabia.Automation.Tests.Authentication;

/// <summary>
/// What a logged-out visitor sees around authentication. Nothing here logs in, requests an OTP or logs out:
/// the login popup is opened and inspected only (same safe checks as carabia-ui-tests LoginFormSmokeTests / ProfileAccessTests).
/// </summary>
[TestFixture]
[Category(TestCategories.Public)]
[Category(TestCategories.Regression)]
[Parallelizable(ParallelScope.Self)]
public sealed class AnonymousAccessTests : UiTestBase
{
    [Test]
    public void Visitor_Is_Reported_Anonymous_By_AuthStatus()
    {
        new HomePage(Session).Open();
        var status = Session.AuthStatus();
        Assert.That(status.IsAnonymous, Is.True, $"A fresh browser must be anonymous. AuthStatus: {status}");
    }

    [Test]
    public void Sign_In_Button_Opens_Phone_Login_Popup_Without_Requesting_An_Otp()
    {
        var home = new HomePage(Session);
        home.Open();
        Assert.That(home.Header.HasSignInButton, Is.True, "Visitors should see the Sign in button (#profileBtn).");
        home.LoginPopup.Open();
        Assert.Multiple(() =>
        {
            Assert.That(home.LoginPopup.IsOpen, Is.True);
            Assert.That(home.LoginPopup.ContinueIsDisabled, Is.True, "Continue must stay disabled until a phone number is entered.");
        });
    }

    [Test]
    public void Login_Popup_Country_Selector_Offers_India_And_Updates_Label()
    {
        var home = new HomePage(Session);
        home.Open();
        home.LoginPopup.Open();
        Assert.That(home.LoginPopup.TrySelectCountry("+91"), Is.True, "Country list should offer India (+91).");
        Assert.Multiple(() =>
        {
            Assert.That(home.LoginPopup.SelectedCountryValue, Is.EqualTo("+91"));
            Assert.That(home.LoginPopup.SelectedCountryText, Is.EqualTo("India +91"));
        });
    }

    [Test]
    public void Login_Continue_Enables_For_A_Phone_Number_But_Is_Never_Clicked()
    {
        var home = new HomePage(Session);
        home.Open();
        home.LoginPopup.Open();
        home.LoginPopup.TrySelectCountry("+91");
        // Fictitious number: only the client-side enable state is checked. Continue is NOT clicked, so no OTP is sent.
        Assert.That(home.LoginPopup.ContinueEnablesForSampleNumber("9876543210"), Is.True);
    }

    [Test]
    [Category(TestCategories.Public)]
    [Category(TestCategories.Smoke)]
    public void Login_Popup_Country_Dropdown_Has_India_Option()
    {
        var home = new HomePage(Session);
        home.Open();
        home.LoginPopup.Open();
        Assert.That(home.LoginPopup.TrySelectCountry("+91"), Is.True,
            "Country dropdown should list India (+91).");
        Assert.Multiple(() =>
        {
            Assert.That(home.LoginPopup.SelectedCountryValue, Is.EqualTo("+91"));
            Assert.That(home.LoginPopup.SelectedCountryText, Does.Contain("India").And.Contain("+91"),
                "Label should update to show India and +91.");
        });
    }

    [Test]
    [Category(TestCategories.Public)]
    [Category(TestCategories.Smoke)]
    public void Login_Popup_Phone_Input_Exists_And_Continue_Button_Starts_Disabled()
    {
        var home = new HomePage(Session);
        home.Open();
        home.LoginPopup.Open();
        Assert.Multiple(() =>
        {
            Assert.That(home.LoginPopup.HasPhoneInput, Is.True, "Phone input must be visible.");
            Assert.That(home.LoginPopup.HasContinueButton, Is.True, "Continue button must be present.");
            Assert.That(home.LoginPopup.ContinueIsDisabled, Is.True,
                "Continue must be disabled when no phone number is entered.");
        });
    }

    [Test]
    public void Identity_Route_Redirects_Visitor_To_Home_With_Login_Prompt()
    {
        Session.Open("/identity");
        Assert.That(Session.Driver.Url, Does.Contain("login=true"), "IdentityController.Index sends visitors to /?login=true.");
    }

    [TestCaseSource(typeof(TestData), nameof(TestData.PersonalRoutes))]
    public void Personal_Page_Shows_Login_Instead_Of_Private_Content_For_Visitor(string path)
    {
        Session.Open(path);
        var popup = new LoginPopupComponent(Session);
        popup.WaitUntilOpen($"a visitor opened {path}");
        Assert.Multiple(() =>
        {
            Assert.That(new GenericPage(Session).RendersPersonalContent, Is.False, "Personal navigation or settings rendered for a visitor.");
            Assert.That(Session.AuthStatus().IsAnonymous, Is.True);
        });
    }

    [TestCase("/administrator/sellings")]
    [TestCase("/administrator/faq")]
    [TestCase("/administrator/payments")]
    public void Administrator_Area_Is_Not_Available_To_Visitor(string path)
    {
        Session.Open(path);
        Assert.Multiple(() =>
        {
            Assert.That(UrlHelper.PathOf(Session.Driver.Url), Does.Not.StartWith("/administrator"), "Visitor reached the admin area.");
            Assert.That(new AdminPage(Session).HasLayout, Is.False, "Admin navigation rendered for a visitor.");
        });
    }

    [Test]
    public void Administrator_Login_Form_Is_Shown_But_Not_Filled()
    {
        var page = new AdminLoginPage(Session);
        page.Open();
        Assert.That(page.HasLoginForm, Is.True, "IdentityController.Authorization should render the administrator login form.");
    }
}
