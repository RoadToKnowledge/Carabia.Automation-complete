using Carabia.Automation.Tests.Base;

namespace Carabia.Automation.Tests.Search;

/// <summary>
/// HomeController.Search is POST-only (route "search"); there is no search box in the current header,
/// so it is checked over HTTP. /System/SearchSuggestions (POST) backs the legacy suggestion script.
/// </summary>
[TestFixture]
[Category(TestCategories.Public)]
[Category(TestCategories.Search)]
[Category(TestCategories.Http)]
[Category(TestCategories.Regression)]
[Parallelizable(ParallelScope.Self)]
public sealed class TextSearchTests : UiTestBase
{
    private static IEnumerable<TestCaseData> Queries() =>
        TestData.Search.TextSearchQueries.Select(q => new TestCaseData(q).SetArgDisplayNames(q));

    [TestCaseSource(nameof(Queries))]
    public async Task Text_Search_Post_Returns_Results_Page(string query)
    {
        using var http = NewHttpProbe();
        var response = await http.PostFormAsync("/search", new Dictionary<string, string> { ["query"] = query });
        Assert.Multiple(() =>
        {
            Assert.That(response.IsSuccess, Is.True, response.ToString());
            Assert.That(response.Body, Does.Contain(query).IgnoreCase, "The results page should echo the query.");
        });
    }

    [Test]
    public async Task Search_Suggestions_Endpoint_Answers()
    {
        using var http = NewHttpProbe();
        var response = await http.PostFormAsync("/System/SearchSuggestions", new Dictionary<string, string> { ["query"] = "au" });
        Assert.That(response.IsSuccess, Is.True, response.ToString());
    }
}
