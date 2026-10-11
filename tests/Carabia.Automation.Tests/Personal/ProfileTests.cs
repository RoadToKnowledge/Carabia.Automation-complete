using Carabia.Automation.Framework.Configuration;
using Carabia.Automation.Framework.Pages.Personal;
using Carabia.Automation.Tests.Base;

namespace Carabia.Automation.Tests.Personal;

/// <summary>
/// /personal/settings (ported from carabia-ui-tests ProfileTests). Read/edit-state checks always run; the save test
/// changes the test account's names and restores them, so it requires Execution.AllowProfileWriteTests=true.
/// No email code is sent, no phone change, no payment methods.
/// </summary>
[TestFixture]
[Category(TestCategories.Personal)]
[Category(TestCategories.Regression)]
public sealed class ProfileTests : BuyerTestBase
{
    private ProfilePage Open()
    {
        var page = new ProfilePage(Session);
        page.Open();
        return page;
    }

    [Test]
    public void Personal_Information_Is_Shown_Read_Only_With_Settings_Active()
    {
        var page = Open();
        Assert.Multiple(() =>
        {
            Assert.That(page.TitleText, Is.EqualTo("Personal Information"));
            Assert.That(page.ActiveNavigationText, Is.EqualTo("Settings"));
            foreach (var name in ProfilePage.Fields)
            {
                Assert.That(page.FieldValue(name), Is.Not.Empty, $"'{name}' should contain the signed-in client's details.");
                Assert.That(page.IsReadOnly(name), Is.True, $"'{name}' should start read-only.");
            }
        });
    }

    [TestCase("FirstName")]
    [TestCase("LastName")]
    [TestCase("User.Email")]
    [TestCase("User.Phone")]
    public void Edit_Unlocks_Only_The_Selected_Field(string name)
    {
        var page = Open();
        page.Edit(name);
        Assert.Multiple(() =>
        {
            Assert.That(page.IsReadOnly(name), Is.False);
            foreach (var other in ProfilePage.Fields.Where(f => f != name))
                Assert.That(page.IsReadOnly(other), Is.True, $"Editing '{name}' must not unlock '{other}'.");
        });
    }

    [TestCase("FirstName")]
    [TestCase("LastName")]
    public void Empty_Name_Fails_Required_Validation(string name)
    {
        var page = Open();
        page.Edit(name);
        page.Field(name).Clear();
        Assert.Multiple(() =>
        {
            Assert.That(page.ValueMissing(name), Is.True);
            Assert.That(page.FormIsValid, Is.False);
        });
    }

    [Test]
    public void Changing_Email_Requires_Verification_Before_Saving()
    {
        var page = Open();
        page.Edit("User.Email");
        page.Replace("User.Email", $"profile-{Guid.NewGuid():N}@example.test");
        Session.WaitFor("the Send code button", _ => page.SendEmailCodeVisible);
        Assert.That(page.Save.Enabled, Is.False, "Save must stay disabled until the new email is verified.");
        // Send code is deliberately not clicked.
    }

    [Test]
    public void Reload_Discards_Unsaved_Name_Change()
    {
        var page = Open();
        var original = page.FieldValue("FirstName");
        page.Edit("FirstName");
        page.Replace("FirstName", $"Unsaved{Guid.NewGuid():N}"[..20]);
        page = Open();
        Assert.Multiple(() =>
        {
            Assert.That(page.FieldValue("FirstName"), Is.EqualTo(original));
            Assert.That(page.IsReadOnly("FirstName"), Is.True);
        });
    }

    [Test]
    [Category(TestCategories.WritesData)]
    public void Saved_Names_Persist_And_Original_Profile_Is_Restored()
    {
        RequireWritePermission(WriteKind.ProfileWrite);
        var page = Open();
        var originalFirst = page.FieldValue("FirstName");
        var originalLast = page.FieldValue("LastName");
        var originalForm = page.CaptureOriginalForm();
        var action = page.FormAction;
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var saveAttempted = false;
        try
        {
            page.Edit("FirstName");
            page.Replace("FirstName", $"UiTest{suffix}");
            page.Edit("LastName");
            page.Replace("LastName", $"Profile{suffix}");
            Session.WaitFor("Save to be enabled", _ => page.Save.Displayed && page.Save.Enabled);
            saveAttempted = true;
            page.SaveAndWaitForReload();
            page = Open();
            Assert.Multiple(() =>
            {
                Assert.That(page.FieldValue("FirstName"), Is.EqualTo($"UiTest{suffix}"));
                Assert.That(page.FieldValue("LastName"), Is.EqualTo($"Profile{suffix}"));
            });
        }
        finally
        {
            if (saveAttempted)
            {
                Assert.That(page.RestoreOriginalForm(action, originalForm), Is.True,
                    "PROFILE CLEANUP FAILED: restore the test account's names manually.");
                page = Open();
                Assert.That(page.FieldValue("FirstName"), Is.EqualTo(originalFirst));
                Assert.That(page.FieldValue("LastName"), Is.EqualTo(originalLast));
            }
        }
    }
}
