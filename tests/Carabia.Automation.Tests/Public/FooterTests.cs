using Carabia.Automation.Framework.Components.Common;
using Carabia.Automation.Framework.Utilities;
using Carabia.Automation.Tests.Base;

namespace Carabia.Automation.Tests.Public;

/// <summary>Shared footer, ported from carabia-ui-tests FooterLinkTests (proven on dev.carabiacars.com).</summary>
[TestFixture]
[Category(TestCategories.Public)]
[Category(TestCategories.Regression)]
[Parallelizable(ParallelScope.Self)]
public sealed class FooterTests : UiTestBase
{
    private FooterComponent OpenFooter()
    {
        Session.Open("/sellings");
        var footer = new FooterComponent(Session);
        footer.ScrollIntoView();
        return footer;
    }

    [TestCaseSource(typeof(TestData), nameof(TestData.FooterInternalLinks))]
    public async Task Footer_Internal_Link_Opens_Expected_Page(string label, string path)
    {
        var footer = OpenFooter();
        var link = footer.LinkByText(label);
        Assert.That(link, Is.Not.Null, $"Footer is missing the '{label}' link.");
        var expected = Session.Url(path);
        Assert.That(link!.GetDomProperty("href"), Is.EqualTo(expected.AbsoluteUri), $"Footer '{label}' points elsewhere.");

        using var http = NewHttpProbe();
        var response = await http.GetAsync(path);
        Assert.That(response.IsSuccess, Is.True, $"Footer '{label}': {response}");

        Step($"Click footer: {label}");
        new ElementActions(Session.Driver, Session.DefaultTimeout).Click(link);
        Session.WaitFor($"footer '{label}' to open {expected}", d => new Uri(d.Url) == expected, Session.LongTimeout);
        Assert.That(new Framework.Pages.Public.GenericPage(Session).MainContentText, Is.Not.Empty, $"Footer '{label}' opened {expected}, but the page content is empty.");
    }

    [Test]
    public void Every_Footer_Link_Has_A_Usable_Destination()
    {
        var links = OpenFooter().Links();
        Assert.That(links, Is.Not.Empty);
        var errors = new List<string>();
        foreach (var link in links)
        {
            var label = link.Text.Trim();
            if (label.Length == 0) label = link.GetDomAttribute("title") ?? link.GetDomAttribute("class") ?? "unnamed link";
            var raw = link.GetDomAttribute("href");
            if (string.IsNullOrWhiteSpace(raw) || raw.Trim() == "#")
                errors.Add($"'{label}' has an empty or placeholder destination '{raw}'.");
            else if (!Uri.TryCreate(link.GetDomProperty("href"), UriKind.Absolute, out var uri) || uri.Scheme is not ("http" or "https" or "mailto" or "tel"))
                errors.Add($"'{label}' has an unsupported destination '{raw}'.");
        }
        Assert.That(errors, Is.Empty, "Footer link problems:\n" + string.Join("\n", errors));
    }

    [TestCaseSource(typeof(TestData), nameof(TestData.FooterExternalLinks))]
    public void Footer_Contact_And_Social_Links_Point_To_Expected_Destination(string label, string selector, string href, bool newTab)
    {
        var links = OpenFooter().LinksMatching(selector);
        Assert.That(links, Has.Count.EqualTo(1), $"Expected one footer {label} link.");
        var link = links[0];
        Assert.Multiple(() =>
        {
            Assert.That(link.Displayed, Is.True, $"Footer {label} link is hidden.");
            Assert.That(link.GetDomProperty("href"), Is.EqualTo(href));
            if (newTab)
                Assert.That(link.GetDomAttribute("target"), Is.EqualTo("_blank"), $"Footer {label} should open in a new tab.");
        });
        // External apps/sites are deliberately not opened.
    }
}
