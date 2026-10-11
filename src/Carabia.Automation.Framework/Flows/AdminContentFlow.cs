using Carabia.Automation.Framework.Base;
using Carabia.Automation.Framework.Pages.Admin;
using Carabia.Automation.Framework.Utilities;

namespace Carabia.Automation.Framework.Flows;

/// <summary>Admin create -> edit -> remove of an AUTO-marked FAQ category (Destructive, guarded by the caller).</summary>
public sealed class AdminContentFlow(BrowserSession session) : FlowBase(session)
{
    public string CreateFaqCategory(string name)
    {
        var form = new AdminEditFormPage(Session);
        form.OpenPath(FaqAdminPage.NewCategoryPath, "the new FAQ category form");
        form.Fill("Name", name);
        form.Save();
        var index = new FaqAdminPage(Session);
        Session.WaitFor($"the new category '{name}' in the FAQ list", _ => index.HasCategoryRow(name), Session.LongTimeout);
        return index.EditHrefForCategory(name);
    }

    public void RenameFaqCategory(string currentName, string newName)
    {
        if (!RandomDataGenerator.IsAutomationMarker(currentName) || !RandomDataGenerator.IsAutomationMarker(newName))
            throw new InvalidOperationException("Only AUTO-marked automation categories may be edited.");
        var index = new FaqAdminPage(Session);
        index.OpenPath(FaqAdminPage.IndexPath, "the FAQ admin list");
        var edit = index.EditHrefForCategory(currentName);
        var form = new AdminEditFormPage(Session);
        form.OpenPath(new Uri(edit).PathAndQuery, "the FAQ category edit form");
        form.Fill("Name", newName);
        form.Save();
    }

    public void RemoveFaqCategory(string name)
    {
        var index = new FaqAdminPage(Session);
        index.OpenPath(FaqAdminPage.IndexPath, "the FAQ admin list");
        if (index.HasCategoryRow(name))
            index.RemoveCategory(name);
    }
}
