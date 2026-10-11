using Carabia.Automation.Framework.Pages.Public;
using Carabia.Automation.Framework.Utilities;
using Carabia.Automation.Tests.Base;

namespace Carabia.Automation.Tests.Public;

/// <summary>UseStatusCodePagesWithReExecute("/System/NotFoundPage") and route constraints returning 404.</summary>
[TestFixture]
[Category(TestCategories.Public)]
[Category(TestCategories.Seo)]
[Category(TestCategories.Regression)]
[Parallelizable(ParallelScope.Self)]
public sealed class ErrorPageTests : UiTestBase
{
    [TestCase("/automation-this-page-does-not-exist")]
    [TestCase("/sellings/999999999_not-a-car")]
    [TestCase("/brand/automation-unknown-brand")]
    public async Task Unknown_Route_Returns_Http_404(string path)
    {
        using var http = NewHttpProbe();
        var response = await http.GetAsync(path);
        Assert.That(response.Code, Is.EqualTo(404), response.ToString());
    }

    [Test]
    public void Unknown_Route_Shows_Carabia_Not_Found_Page_With_Way_Home()
    {
        var page = new ErrorPage(Session);
        page.OpenPath("/automation-this-page-does-not-exist");
        Assert.Multiple(() =>
        {
            Assert.That(page.IsDisplayed, Is.True, "The #notfound page was not rendered.");
            Assert.That(page.MessageText, Does.Contain("404"));
            Assert.That(page.Title, Is.EqualTo("Page not found"));
        });
        page.ClickHomeLink();
        WaitHelper.ForUrl(Session.Driver, u => u.AbsolutePath == "/", "the home page", Session.LongTimeout);
    }

    [Test]
    public async Task Payment_Page_For_Missing_Listing_Returns_404()
    {
        using var http = NewHttpProbe();
        var response = await http.GetAsync("/Payment/PaymentDetails?sellingId=999999999");
        // CanonicalMiddleware first lower-cases the path (301), then PaymentController returns NotFound.
        Assert.That(response.Code, Is.EqualTo(404), response.ToString());
    }
}
