using System.Xml.Linq;
using Carabia.Automation.Framework.Components.Common;
using Carabia.Automation.Framework.Pages.Public;
using Carabia.Automation.Framework.Utilities;
using Carabia.Automation.Tests.Base;

namespace Carabia.Automation.Tests.Seo;

/// <summary>
/// /sitemap_xml (SystemController.SitemapXml via SitemapBuilder), /sitemap (HTML) and the shared
/// sitemap-links section (_SitemapLinksPartial, ported from carabia-ui-tests SitemapLinkTests).
/// </summary>
[TestFixture]
[Category(TestCategories.Public)]
[Category(TestCategories.Seo)]
[Category(TestCategories.Regression)]
[Parallelizable(ParallelScope.Self)]
public sealed class SitemapTests : UiTestBase
{
    private static readonly XNamespace SitemapNs = "http://www.sitemaps.org/schemas/sitemap/0.9";

    private async Task<List<Uri>> ReadSitemapLocations()
    {
        using var http = NewHttpProbe();
        var response = await http.GetAsync("/sitemap_xml");
        Assert.That(response.IsSuccess, Is.True, response.ToString());
        Assert.That(response.ContentType, Does.Contain("xml"), "sitemap_xml must be served as XML.");
        var document = XDocument.Parse(response.Body);
        Assert.That(document.Root?.Name.LocalName, Is.EqualTo("urlset"));
        return document.Descendants(SitemapNs + "loc").Select(l => new Uri(l.Value.Trim())).ToList();
    }

    [Test]
    public async Task Sitemap_Xml_Is_A_Valid_Urlset_With_Unique_Absolute_Urls()
    {
        var locations = await ReadSitemapLocations();
        Assert.Multiple(() =>
        {
            Assert.That(locations, Has.Count.GreaterThan(10));
            Assert.That(locations.Select(l => l.AbsoluteUri), Is.Unique, "Duplicate <loc> entries.");
            Assert.That(locations.All(l => l.Scheme == "https"), Is.True, "Every <loc> should be https.");
        });
    }

    [Test]
    public async Task Sitemap_Xml_Contains_Core_And_Collection_Pages()
    {
        var paths = (await ReadSitemapLocations()).Select(l => l.AbsolutePath.TrimEnd('/')).ToHashSet(StringComparer.OrdinalIgnoreCase);
        // General/SellingCollections: "Adding an entry makes the page routable, titled and indexable."
        var missing = new[] { "/sellings" }.Concat(TestData.Search.QuickFilterCollections.Keys.Select(k => $"/sellings/{k}"))
            .Where(p => !paths.Contains(p)).ToList();
        Assert.That(missing, Is.Empty, "Missing from sitemap_xml:\n" + string.Join("\n", missing));
    }

    [Test]
    public async Task Sample_Of_Sitemap_Paths_Respond_On_This_Environment()
    {
        var locations = await ReadSitemapLocations();
        using var http = NewHttpProbe();
        // <loc> uses the production host (SitemapBuilder.BaseUrl); the same PATH is checked on the configured environment.
        var sample = locations.Where((_, i) => i % Math.Max(1, locations.Count / 40) == 0).Take(40).ToList();
        var failures = new List<string>();
        foreach (var location in sample)
        {
            var result = await http.GetAsync(location.PathAndQuery);
            if (!result.IsSuccess) failures.Add(result.ToString());
        }
        Assert.That(failures, Is.Empty, $"{failures.Count} of {sample.Count} sampled sitemap URLs failed:\n" + string.Join("\n", failures));
    }

    [Test]
    public void Html_Sitemap_Shows_Title_And_Links()
    {
        var page = new SitemapPage(Session);
        page.Open();
        Assert.Multiple(() =>
        {
            Assert.That(page.TitleText, Is.EqualTo("Site Map"));
            Assert.That(page.LinkData(), Has.Count.GreaterThan(5));
        });
    }

    [TestCaseSource(nameof(SectionPages))]
    public void Shared_Sitemap_Section_Renders_All_Six_Groups(string path)
    {
        Session.Open(path);
        var section = new SitemapLinksComponent(Session);
        Assert.That(section.GroupTitles(), Is.EqualTo(SitemapLinksComponent.Groups));
        Assert.Multiple(() =>
        {
            foreach (var group in SitemapLinksComponent.Groups)
                Assert.That(section.LinksIn(group), Is.Not.Empty, $"Group '{group}' has no links on {path}.");
        });
    }

    private static IEnumerable<TestCaseData> SectionPages() =>
        TestData.Pages.SitemapSectionPages.Select(p => new TestCaseData(p).SetArgDisplayNames(p));

    [TestCaseSource(typeof(TestData), nameof(TestData.SitemapGroups))]
    public async Task Every_Link_In_Sitemap_Group_Is_Local_Unique_And_Responds(string group, string prefix, int minimum, string clickPath)
    {
        Session.Open("/sellings");
        var links = new SitemapLinksComponent(Session).LinkDataIn(group);
        Assert.That(links, Has.Count.GreaterThanOrEqualTo(minimum), $"'{group}' needs at least {minimum} links.");
        var errors = new List<string>();
        var seen = new HashSet<string>();
        using var http = NewHttpProbe();
        foreach (var (label, href) in links)
        {
            var context = $"'{group}' / '{label}' -> {href}";
            if (string.IsNullOrWhiteSpace(label)) errors.Add($"{context}: empty text.");
            if (!Uri.TryCreate(href, UriKind.Absolute, out var uri) || !UrlHelper.IsSameHost(href, Settings.BaseUrl)
                || !uri.AbsolutePath.StartsWith(prefix, StringComparison.Ordinal) || uri.Fragment.Length > 0)
            {
                errors.Add($"{context}: expected a local '{prefix}' destination.");
                continue;
            }
            if (!seen.Add(uri.AbsoluteUri)) errors.Add($"{context}: duplicate.");
            var result = await http.GetAsync(uri.AbsoluteUri);
            if (!result.IsSuccess) errors.Add($"{context}: {result}");
            else if (result.FinalUri != uri) errors.Add($"{context}: redirected to {result.FinalUri}.");
        }
        Assert.That(errors, Is.Empty, string.Join("\n", errors));
    }

    [TestCaseSource(typeof(TestData), nameof(TestData.SitemapGroups))]
    public void Clicking_A_Sitemap_Group_Link_Opens_Its_Page(string group, string prefix, int minimum, string clickPath)
    {
        Session.Open("/sellings");
        var expected = Session.Url(clickPath);
        var link = new SitemapLinksComponent(Session).LinksIn(group).SingleOrDefault(a => a.GetDomProperty("href") == expected.AbsoluteUri);
        if (link is null) Assert.Inconclusive($"'{group}' has no link to {clickPath} on this environment.");
        var page = new GenericPage(Session);
        page.ClickAndWaitForNavigation(link!, $"navigation to {clickPath}");
        Assert.Multiple(() =>
        {
            Assert.That(new Uri(Session.Driver.Url), Is.EqualTo(expected));
            Assert.That(page.MainContentText, Is.Not.Empty);
        });
    }
}
