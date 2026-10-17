using Carabia.Automation.Framework.Pages.Admin;
using Carabia.Automation.Framework.Pages.Personal;
using Carabia.Automation.Tests.Base;

namespace Carabia.Automation.Tests.Smoke;

/// <summary>Smoke checks after manual authentication: the role lands in its area. Requires saved/manual sessions.</summary>
[TestFixture]
[Category(TestCategories.Smoke)]
public sealed class BuyerSmokeTests : BuyerTestBase
{
    [Test]
    public void Buyer_Session_Is_An_Authenticated_Client()
    {
        var status = Session.AuthStatus();
        Assert.That(status.IsAuthenticated && status.IsClient, Is.True, $"AuthStatus: {status}");
    }

    [Test]
    public void Buyer_Can_Open_My_Garage()
    {
        var garage = new MyGaragePage(Session);
        garage.OpenTab(MyGaragePage.GarageTab.Buying);
        Assert.That(garage.ActiveTabText, Is.EqualTo("Buying"));
    }

    [TestCase("/personal/buying")]
    [TestCase("/personal/settings")]
    [TestCase("/personal/notifications")]
    public void Buyer_Personal_Page_Loads(string path)
    {
        Session.Open(path);
        var page = new Framework.Pages.Public.GenericPage(Session);
        Assert.Multiple(() =>
        {
            Assert.That(page.ShowsBrowserErrorPage, Is.False, $"{path}: browser error.");
            Assert.That(page.IsNotFoundPage, Is.False, $"{path}: 404 page.");
            Assert.That(page.BodyText.Trim(), Is.Not.Empty, $"{path}: empty body.");
        });
    }
}

[TestFixture]
[Category(TestCategories.Smoke)]
public sealed class SellerSmokeTests : SellerTestBase
{
    [Test]
    public void Seller_Session_Is_An_Authenticated_Client()
    {
        var status = Session.AuthStatus();
        Assert.That(status.IsAuthenticated && status.IsClient, Is.True, $"AuthStatus: {status}");
    }

    [Test]
    public void Seller_Can_Open_Selling_Tab_Of_My_Garage()
    {
        var garage = new MyGaragePage(Session);
        garage.OpenTab(MyGaragePage.GarageTab.Selling);
        Assert.That(garage.ActiveTabText, Is.EqualTo("Selling"));
    }

    [TestCase("/personal/selling")]
    [TestCase("/personal/payments")]
    public void Seller_Personal_Page_Loads(string path)
    {
        Session.Open(path);
        var page = new Framework.Pages.Public.GenericPage(Session);
        Assert.Multiple(() =>
        {
            Assert.That(page.ShowsBrowserErrorPage, Is.False, $"{path}: browser error.");
            Assert.That(page.IsNotFoundPage, Is.False, $"{path}: 404 page.");
            Assert.That(page.BodyText.Trim(), Is.Not.Empty, $"{path}: empty body.");
        });
    }
}

[TestFixture]
[Category(TestCategories.Smoke)]
public sealed class AdminSmokeTests : AdminTestBase
{
    [Test]
    public void Administrator_Can_Open_Sellings_Module()
    {
        var page = new AdminPage(Session);
        page.OpenPath("/administrator/sellings", "Administrator > Sellings");
        Assert.Multiple(() =>
        {
            Assert.That(page.HasLayout, Is.True, "Admin layout (left navigation, top bar) is missing.");
            Assert.That(page.HeadingTexts(), Does.Contain("Sellings"));
        });
    }

    [Test]
    public void Administrator_Can_Open_Payments_Module()
    {
        var page = new AdminPage(Session);
        page.OpenPath("/administrator/payments", "Administrator > Payments");
        Assert.Multiple(() =>
        {
            Assert.That(page.HasLayout, Is.True, "Admin layout is missing.");
            Assert.That(page.HeadingTexts(), Does.Contain("Payments"));
        });
    }
}
