namespace Carabia.Automation.Framework.Models.TestData;

/// <summary>testdata/Admin/AdminModules.json is not needed: the module list is code (AdminModuleCatalog). This holds the CRUD data.</summary>
public sealed class AdminTestData
{
    /// <summary>FAQ category name; "{marker}" becomes an AUTO-... marker. Used only by the guarded destructive CRUD test.</summary>
    public string FaqCategoryNameTemplate { get; set; } = "{marker} automation category";
    public string FaqQuestionTemplate { get; set; } = "{marker} automation question?";
    public string FaqAnswer { get; set; } = "Created and removed by Carabia.Automation. Safe to delete.";
}
