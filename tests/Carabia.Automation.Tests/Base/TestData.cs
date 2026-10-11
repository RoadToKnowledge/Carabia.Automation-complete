using Carabia.Automation.Framework.Models.TestData;
using Carabia.Automation.Framework.Utilities;

namespace Carabia.Automation.Tests.Base;

/// <summary>Typed access to testdata/*.json, loaded once. Also used as TestCaseSource providers.</summary>
public static class TestData
{
    private static readonly Lazy<SellVehiclesFile> VehiclesFile = new(() => TestDataReader.Read<SellVehiclesFile>("Vehicles/SellVehicles.json"));
    private static readonly Lazy<SearchTestData> SearchFile = new(() => TestDataReader.Read<SearchTestData>("Search/SearchCases.json"));
    private static readonly Lazy<OfferTestData> OfferFile = new(() => TestDataReader.Read<OfferTestData>("Offers/OfferData.json"));
    private static readonly Lazy<PaymentTestData> PaymentFile = new(() => TestDataReader.Read<PaymentTestData>("Payments/StripeTestData.json"));
    private static readonly Lazy<AdminTestData> AdminFile = new(() => TestDataReader.Read<AdminTestData>("Admin/AdminData.json"));
    private static readonly Lazy<PublicPagesData> PagesFile = new(() => TestDataReader.Read<PublicPagesData>("Public/PublicPages.json"));

    public static VehicleTestData Vehicle => VehiclesFile.Value.Default;
    public static IReadOnlyList<string> InvalidVins => VehiclesFile.Value.InvalidVins;
    public static SearchTestData Search => SearchFile.Value;
    public static OfferTestData Offers => OfferFile.Value;
    public static PaymentTestData Payments => PaymentFile.Value;
    public static AdminTestData Admin => AdminFile.Value;
    public static PublicPagesData Pages => PagesFile.Value;

    // ---- TestCaseSource providers (evaluated at discovery; failures show as a broken test case, not a crash) ----
    public static IEnumerable<TestCaseData> MakesWithListings() =>
        Search.MakesWithListings.Select(m => new TestCaseData(m).SetArgDisplayNames(m));

    public static IEnumerable<TestCaseData> QuickFilterCollections() =>
        Search.QuickFilterCollections.Select(kv => new TestCaseData(kv.Key, kv.Value).SetArgDisplayNames(kv.Key));

    public static IEnumerable<TestCaseData> PrefixCollections() =>
        Search.PrefixCollections.Select(c => new TestCaseData(c.Path, c.ExpectedHeading).SetArgDisplayNames(c.Path));

    public static IEnumerable<TestCaseData> FaqLandingPages() =>
        Search.FaqLandingPages.Select(c => new TestCaseData(c.Path, c.ExpectedHeading).SetArgDisplayNames(c.Path));

    public static IEnumerable<TestCaseData> SortCases() =>
        Search.SortCases.Select(s => new TestCaseData(s.Value, s.Field, s.Descending).SetArgDisplayNames(s.Value));

    public static IEnumerable<TestCaseData> InvalidVinCases() =>
        InvalidVins.Select(v => new TestCaseData(v).SetArgDisplayNames(v));

    public static IEnumerable<TestCaseData> PublicRoutes() =>
        Pages.PublicRoutes.Select(r => new TestCaseData(r).SetArgDisplayNames(r));

    public static IEnumerable<TestCaseData> InfoPages() =>
        Pages.InfoPageAliases.Select(a => new TestCaseData(a).SetArgDisplayNames(a));

    public static IEnumerable<TestCaseData> ArticleCategories() =>
        Pages.ArticleCategories.Select(a => new TestCaseData(a).SetArgDisplayNames(a));

    public static IEnumerable<TestCaseData> FooterInternalLinks() =>
        Pages.FooterInternalLinks.Select(l => new TestCaseData(l.Label, l.Path).SetArgDisplayNames(l.Label));

    public static IEnumerable<TestCaseData> FooterExternalLinks() =>
        Pages.FooterExternalLinks.Select(l => new TestCaseData(l.Label, l.Selector, l.Href, l.NewTab).SetArgDisplayNames(l.Label));

    public static IEnumerable<TestCaseData> SitemapGroups() =>
        Pages.SitemapGroups.Select(g => new TestCaseData(g.Group, g.PathPrefix, g.MinimumLinks, g.ClickPath).SetArgDisplayNames(g.Group));

    public static IEnumerable<TestCaseData> PersonalRoutes() =>
        Pages.PersonalRoutes.Select(r => new TestCaseData(r).SetArgDisplayNames(r));
}

/// <summary>testdata/Public/PublicPages.json - routes and links taken from the Carabia source / proven by carabia-ui-tests.</summary>
public sealed class PublicPagesData
{
    public List<string> PublicRoutes { get; set; } = [];
    public List<string> InfoPageAliases { get; set; } = [];
    public List<string> ArticleCategories { get; set; } = [];
    public List<string> PersonalRoutes { get; set; } = [];
    public List<FooterLink> FooterInternalLinks { get; set; } = [];
    public List<FooterExternalLink> FooterExternalLinks { get; set; } = [];
    public List<SitemapGroupCase> SitemapGroups { get; set; } = [];
    public List<string> SitemapSectionPages { get; set; } = [];
    public string FeaturedHomeListingPath { get; set; } = string.Empty;
}

public sealed class FooterLink
{
    public string Label { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
}

public sealed class FooterExternalLink
{
    public string Label { get; set; } = string.Empty;
    public string Selector { get; set; } = string.Empty;
    public string Href { get; set; } = string.Empty;
    public bool NewTab { get; set; }
}

public sealed class SitemapGroupCase
{
    public string Group { get; set; } = string.Empty;
    public string PathPrefix { get; set; } = string.Empty;
    public int MinimumLinks { get; set; }
    public string ClickPath { get; set; } = string.Empty;
}
