using Carabia.Automation.Framework.Pages.Public;
using Carabia.Automation.Framework.Utilities;
using Carabia.Automation.Tests.Base;

namespace Carabia.Automation.Tests.Seo;

/// <summary>Internal links on key pages must answer with HTTP 2xx (after redirects). External links are not called.</summary>
[TestFixture]
[Category(TestCategories.Public)]
[Category(TestCategories.Seo)]
[Category(TestCategories.Regression)]
[Parallelizable(ParallelScope.Self)]
public sealed class BrokenLinkTests : UiTestBase
{
    [TestCase("/")]
    [TestCase("/sellings")]
    [TestCase("/sitemap")]
    public async Task Internal_Links_On_Page_Respond(string path)
    {
        var page = new GenericPage(Session);
        page.OpenPath(path);
        var hrefs = page.InternalLinkHrefs();
        Assert.That(hrefs, Is.Not.Empty);
        using var http = NewHttpProbe();
        var failures = new List<string>();
        foreach (var href in hrefs)
        {
            var result = await http.GetAsync(href);
            if (!result.IsSuccess) failures.Add(result.ToString());
        }
        Assert.That(failures, Is.Empty, $"{failures.Count} broken internal link(s) on {path}:\n" + string.Join("\n", failures));
    }

    [Test]
    public void Images_On_Home_Page_Load()
    {
        var home = new HomePage(Session);
        home.Open();
        JavaScriptHelper.Execute(Session.Driver, "window.scrollTo(0, document.body.scrollHeight);");
        WaitHelper.ForDocumentReady(Session.Driver, Session.LongTimeout);
        var broken = home.BrokenVisibleImages();
        Assert.That(broken, Is.Empty, "Broken images:\n" + string.Join("\n", broken));
    }
}
