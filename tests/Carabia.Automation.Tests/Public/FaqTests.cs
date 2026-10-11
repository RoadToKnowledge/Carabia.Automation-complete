using Carabia.Automation.Framework.Components.Common;
using Carabia.Automation.Framework.Pages.Public;
using Carabia.Automation.Tests.Base;

namespace Carabia.Automation.Tests.Public;

/// <summary>FAQ accordions on home, brand/model landing pages and listing details (carabia-ui-tests MakeModelFaqTests/FaqChecks).</summary>
[TestFixture]
[Category(TestCategories.Public)]
[Category(TestCategories.Regression)]
[Parallelizable(ParallelScope.Self)]
public sealed class FaqTests : UiTestBase
{
    [Test]
    public void Home_Page_Faqs_Open_And_Close()
    {
        new HomePage(Session).Open();
        var failures = new FaqComponent(Session).CheckSample("Home");
        Assert.That(failures, Is.Empty, string.Join("\n", failures));
    }

    [TestCaseSource(typeof(TestData), nameof(TestData.FaqLandingPages))]
    public void Make_Or_Model_Landing_Page_Faqs_Open_And_Close(string path, string vehicle)
    {
        Session.Open(path);
        Assert.That(Framework.Utilities.UrlHelper.SamePath(Session.Driver.Url, Session.Url(path).AbsoluteUri), Is.True,
            $"{vehicle}: {path} redirected to {Session.Driver.Url}.");
        var heading = new SearchPage(Session).HeadingText;
        Assert.That(heading, Does.Contain(vehicle).IgnoreCase, $"Expected the {vehicle} page.");
        var failures = new FaqComponent(Session).CheckSample(vehicle);
        Assert.That(failures, Is.Empty, string.Join("\n", failures));
    }
}
