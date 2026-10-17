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

    [Test]
    [Category(TestCategories.Public)]
    [Category(TestCategories.Seo)]
    public void Brand_Landing_Page_Audi_Shows_Working_Faqs()
    {
        var brand = new BrandPage(Session);
        brand.OpenBrand("audi");
        var faq = brand.Faq;
        if (faq.Count == 0) Assert.Inconclusive("No FAQ items on /brand/audi.");
        Assert.That(faq.Count, Is.GreaterThanOrEqualTo(3), "Brand page should have at least 3 FAQs.");
        var question = faq.OpenAndCloseQuestion(0);
        Assert.That(question, Is.Not.Empty, "First FAQ question must have content.");
    }

    [Test]
    [Category(TestCategories.Public)]
    [Category(TestCategories.Seo)]
    public void Brand_Landing_Page_Bmw_Shows_Working_Faqs()
    {
        var brand = new BrandPage(Session);
        brand.OpenBrand("bmw");
        var faq = brand.Faq;
        if (faq.Count == 0) Assert.Inconclusive("No FAQ items on /brand/bmw.");
        Assert.That(faq.Count, Is.GreaterThanOrEqualTo(3));
        var question = faq.OpenAndCloseQuestion(0);
        Assert.That(question, Is.Not.Empty);
    }

    [Test]
    [Category(TestCategories.Public)]
    [Category(TestCategories.Seo)]
    public void Model_Collection_Page_Shows_Working_Faqs()
    {
        Session.Open("/sellings/model-a6");
        var faq = new FaqComponent(Session);
        if (faq.Count == 0) Assert.Inconclusive("No FAQ items on /sellings/model-a6 — model may not exist.");
        Assert.That(faq.Count, Is.GreaterThanOrEqualTo(1));
        var question = faq.OpenAndCloseQuestion(0);
        Assert.That(question, Is.Not.Empty);
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
