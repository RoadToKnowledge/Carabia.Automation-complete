using Carabia.Automation.Framework.Pages.Admin;
using Carabia.Automation.Tests.Base;

namespace Carabia.Automation.Tests.Admin;

[TestFixture]
[Category(TestCategories.Admin)]
[Category(TestCategories.Regression)]
public sealed class AdminEditPageReadTests : AdminTestBase
{
    [Test]
    public void Admin_Can_Open_Selling_Edit_Page_And_See_Form_Fields()
    {
        AssertEditFormOpens("/administrator/sellings", "Sellings");
    }

    [Test]
    public void Admin_Can_Open_Client_Edit_Page_And_See_Form()
    {
        AssertEditFormOpens("/administrator/clients", "Clients");
    }

    [Test]
    public void Admin_Can_Open_Banner_Edit_Page_And_See_Form()
    {
        AssertEditFormOpens("/administrator/banners", "Banners");
    }

    [Test]
    public void Admin_Can_Open_News_Edit_Page_And_See_Form()
    {
        AssertEditFormOpens("/administrator/news", "News");
    }

    [Test]
    public void Admin_Can_Open_Info_Page_Edit_And_See_Form()
    {
        AssertEditFormOpens("/administrator/pages", "Pages");
    }

    [Test]
    public void Admin_Can_Open_Dictionary_Body_Type_Edit_Form()
    {
        var page = new AdminPage(Session);
        page.OpenPath("/administrator/dictionary", "Dictionary");
        var edit = page.EditButtonHrefs().FirstOrDefault();
        if (edit is null) Assert.Inconclusive("No dictionary items to edit.");
        var form = new AdminEditFormPage(Session);
        form.OpenPath(new Uri(edit!).PathAndQuery, "BodyType edit");
        Assert.Multiple(() =>
        {
            Assert.That(form.HasPostForm, Is.True);
            Assert.That(form.HasField("Name"), Is.True, "BodyType edit form must have Name field.");
            Assert.That(form.HasField("IsActive"), Is.True, "BodyType edit form must have IsActive field.");
        });
    }

    [Test]
    public void Admin_Can_Open_Manufacturer_Edit_Form()
    {
        var page = new AdminPage(Session);
        page.OpenPath("/administrator/manufacturers", "Manufacturers");
        var edit = page.EditButtonHrefs().FirstOrDefault();
        if (edit is null) Assert.Inconclusive("No manufacturers to edit.");
        var form = new AdminEditFormPage(Session);
        form.OpenPath(new Uri(edit!).PathAndQuery, "Manufacturer edit");
        Assert.Multiple(() =>
        {
            Assert.That(form.HasPostForm, Is.True);
            Assert.That(form.HasField("Name"), Is.True, "Manufacturer edit form must have Name field.");
        });
    }

    [Test]
    public void Admin_Can_Open_Model_Edit_Form()
    {
        var page = new AdminPage(Session);
        page.OpenPath("/administrator/models", "Models");
        var edit = page.EditButtonHrefs().FirstOrDefault();
        if (edit is null) Assert.Inconclusive("No models to edit.");
        var form = new AdminEditFormPage(Session);
        form.OpenPath(new Uri(edit!).PathAndQuery, "Model edit");
        Assert.Multiple(() =>
        {
            Assert.That(form.HasPostForm, Is.True);
            Assert.That(form.HasField("Name"), Is.True, "Model edit form must have Name field.");
            Assert.That(form.HasField("ManufacturerId"), Is.True, "Model edit form must have ManufacturerId field.");
        });
    }

    private void AssertEditFormOpens(string listPath, string moduleName)
    {
        var page = new AdminPage(Session);
        page.OpenPath(listPath, moduleName);
        var edit = page.EditButtonHrefs().FirstOrDefault();
        if (edit is null) Assert.Inconclusive($"No {moduleName.ToLowerInvariant()} to edit on this environment.");
        var form = new AdminEditFormPage(Session);
        form.OpenPath(new Uri(edit!).PathAndQuery, $"{moduleName} edit");
        Assert.Multiple(() =>
        {
            Assert.That(form.IsNotFoundPage, Is.False);
            Assert.That(form.HasPostForm, Is.True, "Edit page must have a POST form.");
            Assert.That(form.HasSaveButton, Is.True, "Edit page must have a Save button.");
            Assert.That(form.HasAntiForgeryToken, Is.True, "Form must have __RequestVerificationToken.");
        });
        // SAVE IS NEVER CLICKED.
    }
}
