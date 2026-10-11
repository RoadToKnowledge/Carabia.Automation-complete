using Carabia.Automation.Framework.Flows;
using Carabia.Automation.Framework.Pages.Public;
using Carabia.Automation.Framework.Utilities;
using Carabia.Automation.Tests.Base;

namespace Carabia.Automation.Tests.Seo;

/// <summary>
/// CanonicalMiddleware (301 to lower-case path, 301 without trailing slash), rel="canonical" tags
/// (hard-coded to https://carabiacars.com + path in the views), titles/meta descriptions.
/// </summary>
[TestFixture]
[Category(TestCategories.Public)]
[Category(TestCategories.Seo)]
[Category(TestCategories.Regression)]
[Parallelizable(ParallelScope.Self)]
public sealed class CanonicalAndRedirectTests : UiTestBase
{
    [TestCase("/Sellings", "/sellings")]
    [TestCase("/SITEMAP", "/sitemap")]
    [TestCase("/Brand/BMW", "/brand/bmw")]
    public async Task Upper_Case_Path_Redirects_Permanently_To_Lower_Case(string path, string expected)
    {
        using var http = NewHttpProbe();
        var response = await http.GetAsync(path, followRedirects: false);
        Assert.Multiple(() =>
        {
            Assert.That(response.Code, Is.EqualTo(301), response.ToString());
            Assert.That(UrlHelper.PathOf(http.Absolute(response.Location!.OriginalString).AbsoluteUri), Is.EqualTo(expected));
        });
    }

    [TestCase("/sellings/", "/sellings")]
    [TestCase("/sitemap/", "/sitemap")]
    public async Task Trailing_Slash_Redirects_Permanently(string path, string expected)
    {
        using var http = NewHttpProbe();
        var response = await http.GetAsync(path, followRedirects: false);
        Assert.Multiple(() =>
        {
            Assert.That(response.Code, Is.EqualTo(301), response.ToString());
            Assert.That(UrlHelper.PathOf(http.Absolute(response.Location!.OriginalString).AbsoluteUri), Is.EqualTo(expected));
        });
    }

    [Test]
    public async Task Query_String_Is_Kept_On_Lower_Case_Redirect()
    {
        using var http = NewHttpProbe();
        var response = await http.GetAsync("/Auctions?auctions_visible=1", followRedirects: false);
        Assert.That(response.Location?.OriginalString, Does.Contain("auctions_visible=1"), response.ToString());
    }

    [TestCase("/", "/")]
    [TestCase("/sellings", "/sellings")]
    [TestCase("/sellings/evs", "/sellings/evs")]
    [TestCase("/auctions", "/auctions")]
    [TestCase("/info/about-us", "/info/about-us")]
    public void Page_Has_Canonical_Link_To_Its_Own_Path(string path, string canonicalPath)
    {
        var page = new GenericPage(Session);
        page.OpenPath(path);
        Assert.That(page.CanonicalHref, Is.Not.Null.And.Not.Empty, $"{path} has no rel=canonical.");
        var canonical = new Uri(page.CanonicalHref!);
        Assert.Multiple(() =>
        {
            Assert.That(canonical.Scheme, Is.EqualTo("https"));
            Assert.That(canonical.AbsolutePath.TrimEnd('/') is { Length: > 0 } p ? p : "/", Is.EqualTo(canonicalPath));
        });
    }

    [Test]
    public void Listing_Detail_Canonical_Matches_Listing_Route()
    {
        var (_, url) = new BrowseCarsFlow(Session).OpenListingFromResults();
        var page = new ListingDetailsPage(Session);
        Assert.That(UrlHelper.PathOf(page.CanonicalHref ?? ""), Is.EqualTo(UrlHelper.PathOf(url)).IgnoreCase);
    }

    [TestCase("/")]
    [TestCase("/sellings")]
    [TestCase("/sellings/suvs")]
    [TestCase("/brand/bmw")]
    public void Page_Has_Title_And_Meta_Description(string path)
    {
        var page = new GenericPage(Session);
        page.OpenPath(path);
        Assert.Multiple(() =>
        {
            Assert.That(page.Title, Is.Not.Empty, "<title> is empty.");
            Assert.That(page.Title.Length, Is.LessThanOrEqualTo(200));
            Assert.That(page.MetaDescription, Is.Not.Null.And.Not.Empty, "meta description is empty.");
            Assert.That(page.FirstH1Text, Is.Not.Empty, "No visible h1.");
        });
    }
}
