using Carabia.Automation.Framework.Configuration;
using Carabia.Automation.Framework.Flows;
using Carabia.Automation.Framework.Pages.Admin;
using Carabia.Automation.Framework.Utilities;
using Carabia.Automation.Tests.Base;
using OpenQA.Selenium;

namespace Carabia.Automation.Tests.Admin;

/// <summary>Admin navigation, selling-offer filter, moderation queues, Autodata page and the guarded FAQ CRUD.</summary>
[TestFixture]
[Category(TestCategories.Regression)]
public sealed class AdminWorkflowTests : AdminTestBase
{
    [Test]
    public void Left_Navigation_Links_All_Open_Admin_Pages()
    {
        var page = new AdminPage(Session);
        page.OpenPath("/administrator/sellings", "Sellings");
        var links = page.LeftNavigation().Where(l => !l.Href.EndsWith("#")).ToList();
        var expected = AdminModuleCatalog.All.Where(m => m.NavText is not null).Select(m => m.NavText!).ToList();
        Assert.That(links.Select(l => l.Text), Is.SupersetOf(expected), "Left navigation is missing modules.");
        var failures = new List<string>();
        foreach (var (text, href) in links)
        {
            page.OpenPath(new Uri(href).PathAndQuery, text);
            if (page.IsNotFoundPage || !page.HasLayout) failures.Add($"{text} -> {href}");
        }
        Assert.That(failures, Is.Empty, "Broken admin navigation:\n" + string.Join("\n", failures));
    }

    [Test]
    public void Selling_Offers_Status_Filter_Shows_Only_That_Status()
    {
        var page = new SellingOffersPage(Session);
        page.OpenPath("/administrator/sellingoffers", "Selling offers");
        Assert.That(page.StatusOptions(), Is.EqualTo(new[] { "", "Active", "Accepted", "Declined" }));
        foreach (var status in new[] { "Active", "Accepted", "Declined" })
        {
            page.FilterBy(status);
            Assert.Multiple(() =>
            {
                Assert.That(page.SelectedStatus, Is.EqualTo(status));
                Assert.That(Session.Driver.Url, Does.Contain($"status={status}"));
                Assert.That(page.RowStatuses().All(s => s == status), Is.True,
                    $"Filter {status} shows other statuses: {string.Join(", ", page.RowStatuses().Distinct())}");
            });
        }
    }

    [Test]
    public void Selling_Offer_Details_Open_From_List()
    {
        var page = new SellingOffersPage(Session);
        page.OpenPath("/administrator/sellingoffers", "Selling offers");
        var href = page.DetailsHrefs().FirstOrDefault();
        if (href is null) Assert.Inconclusive("No selling offers on this environment.");
        page.OpenPath(new Uri(href!).PathAndQuery, "Selling offer details");
        Assert.Multiple(() =>
        {
            Assert.That(page.HeadingTexts(), Has.Some.StartsWith("Selling Offer #"));
            Assert.That(page.TableCount > 0 || page.HeadingTexts().Count > 1, Is.True,
                "Details page should have detail tables or multiple content sections.");
            Assert.That(page.HasLayout, Is.True, "Admin layout missing on offer details page.");
        });
        // Accept / Decline buttons are not clicked: accepting can capture a buyer payment.
    }

    [Test]
    [Category(TestCategories.Admin)]
    public void Selling_Offer_Details_Page_Has_Action_Buttons_Not_Clicked()
    {
        var page = new SellingOffersPage(Session);
        page.OpenPath("/administrator/sellingoffers", "Selling offers");
        var href = page.DetailsHrefs().FirstOrDefault();
        if (href is null) Assert.Inconclusive("No selling offers on this environment.");
        page.OpenPath(new Uri(href!).PathAndQuery, "Selling offer details");
        var buttons = Session.Driver.FindElements(By.CssSelector(".dashboard-content .btn"));
        Assert.That(buttons.Count, Is.GreaterThan(0), "Details page should have action buttons.");
        // Accept / Decline are NEVER clicked: accepting can capture a buyer payment.
    }

    [TestCase("/administrator/moderation/next", "There are no offers available for moderation")]
    [TestCase("/administrator/comments/moderation", "There are no comments available for moderation")]
    public void Moderation_Queue_Shows_Next_Item_Or_Empty_Message(string path, string emptyText)
    {
        var page = new ModerationPage(Session);
        page.OpenPath(path, path);
        if (page.IsEmpty)
            Assert.That(page.EmptyText, Is.EqualTo(emptyText));
        else
            Assert.Multiple(() =>
            {
                Assert.That(page.HasModerationForm, Is.True, "A moderation item must render its form.");
                Assert.That(page.HasApproveAction, Is.True);
                Assert.That(page.HasDeclineAction, Is.True, "A moderation item must offer a Decline action.");
                Assert.That(page.HasContentArea, Is.True, "A moderation item must show content to review.");
            });
        // Approve / Decline / Remove are never clicked on real submissions.
    }

    [Test]
    public void Autodata_Page_Offers_Both_Imports_And_They_Are_Not_Started()
    {
        var page = new AutodataPage(Session);
        page.OpenPath("/administrator/autodata", "Autodata import");
        Assert.Multiple(() =>
        {
            Assert.That(page.ImportUrls(), Is.EquivalentTo(new[] { "/Autodata/ImportTreeTables", "/Autodata/ImportTree" }));
            Assert.That(page.StatusHidden, Is.True);
        });
    }

    [Test]
    public void Test_Drive_Requests_Page_Has_Status_Tabs()
    {
        var page = new AdminPage(Session);
        page.OpenPath("/administrator/testdrive", "Test drives");
        Assert.That(page.TabTexts(), Is.EqualTo(new[] { "Pending", "Approved", "Scheduled" }));
    }

    [Test]
    [Category(TestCategories.Destructive)]
    [Category(TestCategories.WritesData)]
    public void Admin_Can_Create_Rename_And_Remove_An_Automation_Faq_Category()
    {
        RequireWritePermission(WriteKind.Destructive);
        var marker = RandomDataGenerator.AutomationMarker();
        var name = TestData.Admin.FaqCategoryNameTemplate.Replace("{marker}", marker);
        var renamed = name + " (edited)";
        var flow = new AdminContentFlow(Session);
        try
        {
            flow.CreateFaqCategory(name);
            flow.RenameFaqCategory(name, renamed);
            var index = new FaqAdminPage(Session);
            Assert.That(index.HasCategoryRow(renamed), Is.True, "The renamed automation category is not listed.");
        }
        finally
        {
            // Only rows containing this run's AUTO marker are ever removed.
            flow.RemoveFaqCategory(renamed);
            flow.RemoveFaqCategory(name);
        }
        var check = new FaqAdminPage(Session);
        check.OpenPath(FaqAdminPage.IndexPath, "FAQ");
        Assert.That(check.HasCategoryRow(renamed) || check.HasCategoryRow(name), Is.False, "Cleanup failed: the automation category still exists.");
    }

    [Test]
    [Category(TestCategories.Admin)]
    public void Admin_Can_View_Test_Drive_Request_Details()
    {
        var page = new AdminPage(Session);
        page.OpenPath("/administrator/testdrive", "Test drives");

        var tabs = new[] { "Pending", "Approved", "Scheduled" };
        string? detailHref = null;
        foreach (var tab in tabs)
        {
            page.ClickTab(tab);
            var viewLinks = Session.Driver.FindElements(
                By.XPath("//table[contains(@class,'table')]//a[normalize-space(.)='View' or normalize-space(.)='Details']"));
            if (viewLinks.Count > 0)
            {
                detailHref = viewLinks.First().GetDomProperty("href");
                break;
            }
        }
        if (detailHref is null)
            Assert.Inconclusive("No test drive requests found on any tab.");

        page.OpenPath(new Uri(detailHref).PathAndQuery, "Test drive details");
        Assert.Multiple(() =>
        {
            Assert.That(page.HeadingTexts(), Is.Not.Empty, "Details page has no headings.");
            Assert.That(page.HasLayout, Is.True, "Admin layout missing on details page.");
            Assert.That(Session.Driver.FindElements(By.CssSelector(".dashboard-content")).Count,
                Is.GreaterThan(0), "No content area rendered on details page.");
        });
    }
}

/// <summary>A signed-in CLIENT (Buyer) must not reach the Administrator area.</summary>
[TestFixture]
[Category(TestCategories.Admin)]
[Category(TestCategories.Regression)]
public sealed class AdminAccessForClientTests : BuyerTestBase
{
    [TestCase("/administrator/sellings")]
    [TestCase("/administrator/clients")]
    public void Client_Account_Cannot_Open_Administrator_Pages(string path)
    {
        Session.Open(path);
        Assert.That(UrlHelper.PathOf(Session.Driver.Url), Does.Not.StartWith("/administrator"),
            "A client account reached the admin area (cookie auth should send it to '/').");
    }
}
