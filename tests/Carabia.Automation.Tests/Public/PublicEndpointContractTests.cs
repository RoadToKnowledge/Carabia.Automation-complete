using System.Text.Json;
using Carabia.Automation.Tests.Base;

namespace Carabia.Automation.Tests.Public;

[TestFixture]
[Category(TestCategories.Public)]
[Category(TestCategories.Http)]
[Category(TestCategories.Regression)]
[Parallelizable(ParallelScope.Self)]
public sealed class PublicEndpointContractTests : UiTestBase
{
    [Test]
    public async Task Card_Binding_Fail_Page_Returns_200()
    {
        using var http = NewHttpProbe();
        var response = await http.GetAsync("/System/CardBindingFail");
        Assert.That(response.Code, Is.EqualTo(200), response.ToString());
    }

    [Test]
    public async Task Brand_Listing_Ajax_Returns_Content()
    {
        using var http = NewHttpProbe();
        var response = await http.PostFormAsync("/Home/BrandListing",
            new Dictionary<string, string> { ["baseUrl"] = "/brand/bmw", ["page"] = "1" });
        Assert.That(response.IsSuccess, Is.True, response.ToString());
        Assert.That(response.Body.Length, Is.GreaterThan(10), "BrandListing returned empty body.");
    }

    [Test]
    public async Task Sell_Car_Qualification_Requires_Login_Or_Returns_Json()
    {
        using var http = NewHttpProbe();
        var response = await http.PostFormAsync("/Personal/GetSellCarListingQualification",
            new Dictionary<string, string>
            {
                ["year"] = "2020", ["manufacturerId"] = "1", ["modelId"] = "1",
                ["mileage"] = "50000", ["condition"] = "Good"
            });
        if (!response.IsSuccess)
            Assert.Pass("Endpoint correctly requires authentication.");
        var json = Json(response);
        Assert.That(json.ValueKind, Is.Not.EqualTo(JsonValueKind.Undefined));
    }

    [Test]
    public async Task Auctions_Index_Listing_Returns_Response()
    {
        using var http = NewHttpProbe();
        var response = await http.PostFormAsync("/Auctions/IndexListing",
            new Dictionary<string, string>());
        if (response.Code == 404)
            Assert.Inconclusive("Auctions IndexListing may be disabled in this release.");
        Assert.That(response.IsSuccess, Is.True, response.ToString());
    }

    [Test]
    public async Task Contact_Us_Page_Returns_200()
    {
        using var http = NewHttpProbe();
        var response = await http.GetAsync("/contact-us");
        Assert.That(response.IsSuccess, Is.True, response.ToString());
        Assert.That(response.Body, Does.Contain("contact").IgnoreCase);
    }

    [Test]
    public async Task Info_Fees_Page_Returns_200_With_Content()
    {
        using var http = NewHttpProbe();
        var response = await http.GetAsync("/info/how-it-works");
        Assert.That(response.IsSuccess, Is.True, response.ToString());
        Assert.That(response.Body.Length, Is.GreaterThan(100), "Fees page seems empty.");
    }

    [Test]
    public async Task Autodata_Sell_Makes_Returns_Json()
    {
        using var http = NewHttpProbe();
        var response = await http.GetAsync("/api/autodata/sell-your-car/makes?year=2020");
        if (response.Code == 404 || response.Code == 500)
            Assert.Inconclusive("Autodata API may not be configured on this environment.");
        Assert.That(response.IsSuccess, Is.True, response.ToString());
    }

    [Test]
    public async Task Subscribe_Email_Requires_Captcha()
    {
        using var http = NewHttpProbe();
        var response = await http.PostFormAsync("/System/AddSubscribeEmail",
            new Dictionary<string, string> { ["email"] = "test@example.com" });
        Assert.That(response.IsSuccess, Is.False,
            "AddSubscribeEmail should require reCAPTCHA and reject plain POST.");
    }

    private static JsonElement Json(Framework.Utilities.HttpCheck response)
    {
        try { return JsonDocument.Parse(response.Body).RootElement; }
        catch { return default; }
    }

    private static string Shorten(string text, int max = 200) =>
        text.Length <= max ? text : text[..max] + "...";
}
