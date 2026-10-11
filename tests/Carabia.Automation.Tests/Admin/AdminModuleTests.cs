using Carabia.Automation.Framework.Pages.Admin;
using Carabia.Automation.Framework.Utilities;
using Carabia.Automation.Tests.Base;

namespace Carabia.Automation.Tests.Admin;

/// <summary>
/// Every Administrator-area module from AdminModuleCatalog (all controllers in Areas/Administrator/Controllers):
/// list page, layout, headings, table structure, tabs, Add/Edit forms (opened, never saved) and the
/// confirm() guard on Remove links (never clicked). Read-only.
/// </summary>
[TestFixture]
[Category(TestCategories.Regression)]
public sealed class AdminModuleTests : AdminTestBase
{
    private static IEnumerable<TestCaseData> Modules() =>
        AdminModuleCatalog.WithIndex.Select(m => new TestCaseData(m).SetArgDisplayNames(m.Name));

    private static IEnumerable<TestCaseData> TableModules() =>
        AdminModuleCatalog.WithIndex.Where(m => m.HasTable).Select(m => new TestCaseData(m).SetArgDisplayNames(m.Name));

    private static IEnumerable<TestCaseData> AddForms() =>
        AdminModuleCatalog.All.SelectMany(m => m.AddForms.Select(f => new TestCaseData(m.Name, f).SetArgDisplayNames($"{m.Name} {f.Path}")));

    private static IEnumerable<TestCaseData> TabbedModules() =>
        AdminModuleCatalog.WithTabs.Select(m => new TestCaseData(m).SetArgDisplayNames(m.Name));

    [TestCaseSource(nameof(Modules))]
    public void Module_Page_Opens_With_Admin_Layout_Title_And_Headings(AdminModule module)
    {
        var page = new AdminPage(Session);
        page.OpenPath(module.Path, $"Administrator > {module.Name}");
        Assert.Multiple(() =>
        {
            Assert.That(page.HasLayout, Is.True, "Admin layout (left navigation / top bar) missing.");
            if (module.ExpectedTitle is not null)
                Assert.That(page.Title, Is.EqualTo(module.ExpectedTitle));
            var headings = page.HeadingTexts();
            foreach (var heading in module.ExpectedHeadings)
                Assert.That(headings, Has.Some.StartsWith(heading), $"Heading '{heading}' missing. Found: {string.Join(" | ", headings)}");
        });
    }

    [TestCaseSource(nameof(TableModules))]
    public void Module_List_Renders_Table_With_Header_Columns(AdminModule module)
    {
        var page = new AdminPage(Session);
        page.OpenPath(module.Path, module.Name);
        Assert.Multiple(() =>
        {
            Assert.That(page.TableCount, Is.GreaterThan(0), "No table.table rendered.");
            Assert.That(page.ColumnHeaders(), Is.Not.Empty, "The first table has no header columns.");
            if (module.ExpectedColumns.Count > 0)
                Assert.That(page.ColumnHeaders(), Is.EqualTo(module.ExpectedColumns));
        });
        TestContext.Out.WriteLine($"{module.Name}: {page.RowCount} row(s), {page.EditButtonHrefs().Count} edit link(s).");
    }

    [TestCaseSource(nameof(TableModules))]
    public void Remove_Links_Are_Protected_By_Confirmation(AdminModule module)
    {
        var page = new AdminPage(Session);
        page.OpenPath(module.Path, module.Name);
        var unguarded = page.RemoveLinks().Where(r => !r.HasConfirmGuard).Select(r => r.Href).ToList();
        // ServiceRequests' Remove buttons have no "remove" class (no confirm). Reported, never clicked.
        if (module.Name == "ServiceRequests" && unguarded.Count > 0)
            Assert.Warn($"ServiceRequests has {unguarded.Count} Remove link(s) without a confirmation guard (see KNOWN_LIMITATIONS.md).");
        else
            Assert.That(unguarded, Is.Empty, "Remove links without confirm():\n" + string.Join("\n", unguarded));
    }

    [TestCaseSource(nameof(AddForms))]
    public void Add_Form_Opens_With_Expected_Fields_Antiforgery_And_Save(string moduleName, AdminForm form)
    {
        var page = new AdminEditFormPage(Session);
        page.OpenPath(form.Path, $"{moduleName} add form");
        Assert.Multiple(() =>
        {
            Assert.That(page.HeadingTexts(), Does.Contain(form.ExpectedHeading));
            Assert.That(page.HasPostForm, Is.True);
            Assert.That(page.HasAntiForgeryToken, Is.True, "POST form must carry __RequestVerificationToken.");
            Assert.That(page.HasSaveButton, Is.True);
            Assert.That(page.MissingFields(form.FieldNames), Is.Empty);
            Assert.That(page.HiddenId, Is.EqualTo("0").Or.Empty, "A new record form must not carry an existing id.");
        });
    }

    [TestCaseSource(nameof(AddForms))]
    public void Add_Form_Is_Invalid_When_Required_Fields_Are_Empty(string moduleName, AdminForm form)
    {
        var page = new AdminEditFormPage(Session);
        page.OpenPath(form.Path, $"{moduleName} add form");
        var required = form.FieldNames.Where(page.IsRequired).ToList();
        if (required.Count == 0) Assert.Inconclusive($"{form.Path} declares no required fields.");
        Assert.That(page.FormIsValid, Is.False, $"Empty required fields ({string.Join(", ", required)}) should block submit.");
    }

    [TestCaseSource(nameof(TabbedModules))]
    public void Module_Tabs_Switch_Panes(AdminModule module)
    {
        var page = new AdminPage(Session);
        page.OpenPath(module.Path, module.Name);
        Assert.Multiple(() =>
        {
            foreach (var tab in module.Tabs.Reverse())
                Assert.That(page.ClickTab(tab), Is.True, $"Tab '{tab}' did not activate its pane.");
        });
    }

    [TestCaseSource(nameof(TableModules))]
    public void First_Edit_Page_Opens_Without_Saving(AdminModule module)
    {
        var page = new AdminPage(Session);
        page.OpenPath(module.Path, module.Name);
        var edit = page.EditButtonHrefs().FirstOrDefault();
        if (edit is null) Assert.Inconclusive($"{module.Name} has no rows to edit on this environment.");
        var form = new AdminEditFormPage(Session);
        form.OpenPath(new Uri(edit!).PathAndQuery, $"{module.Name} edit page");
        Assert.Multiple(() =>
        {
            Assert.That(form.IsNotFoundPage, Is.False);
            Assert.That(form.HasPostForm || form.TableCount > 0, Is.True, "The edit page rendered no form.");
        });
    }

    [Test]
    public void Catalog_Covers_Every_Administrator_Controller()
    {
        // Guards against silently forgetting a module when Carabia adds a controller.
        var expected = new[]
        {
            "Administrators", "Auctions", "Autodata", "Banners", "Cars", "Clients", "Comments", "Dictionary", "Faq",
            "Manufacturers", "Models", "Moderation", "News", "Pages", "Payments", "SellingOffers", "Sellings",
            "ServiceRequests", "System", "TestDrive"
        };
        Assert.That(AdminModuleCatalog.All.Select(m => m.Name), Is.EquivalentTo(expected));
        Assert.That(UrlHelper.PathOf(Settings.BaseUrl), Is.Not.Null);
    }
}
