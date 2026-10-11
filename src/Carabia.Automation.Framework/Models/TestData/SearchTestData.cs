namespace Carabia.Automation.Framework.Models.TestData;

/// <summary>testdata/Search/SearchCases.json</summary>
public sealed class SearchTestData
{
    /// <summary>Makes that must have active listings on the environment (used by filter tests).</summary>
    public List<string> MakesWithListings { get; set; } = [];

    /// <summary>Quick-filter collections from General/SellingCollections.QuickFilters: slug -> expected heading fragment.</summary>
    public Dictionary<string, string> QuickFilterCollections { get; set; } = new();

    /// <summary>Prefix collections from SellingCollections.PrefixFilters (body-, location-, model-, under-) known to exist.</summary>
    public List<CollectionCase> PrefixCollections { get; set; } = [];

    public List<string> TextSearchQueries { get; set; } = [];

    /// <summary>Brand/model landing pages with FAQs (from carabia-ui-tests MakeModelFaqTests).</summary>
    public List<CollectionCase> FaqLandingPages { get; set; } = [];

    /// <summary>Sorting options of the sidebar (value of input[name='crb_sort']), field compared and direction.</summary>
    public List<SortCase> SortCases { get; set; } = [];
}

public sealed class CollectionCase
{
    public string Path { get; set; } = string.Empty;
    public string ExpectedHeading { get; set; } = string.Empty;
}

public sealed class SortCase
{
    public string Value { get; set; } = string.Empty;
    public string Field { get; set; } = "price";
    public bool Descending { get; set; }
}
