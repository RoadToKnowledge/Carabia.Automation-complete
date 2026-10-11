namespace Carabia.Automation.Framework.Pages.Admin;

/// <summary>
/// Every controller in Carabia.Web/Areas/Administrator/Controllers, with what its views really contain
/// (titles from ViewBag.PageTitle, headings from .pageheader-title / .card-header, Add forms and their fields).
/// This drives the parameterised admin tests, so a module cannot be silently forgotten.
///
/// Remove/Delete actions in this area are plain GET links (e.g. /administrator/faq/remove/5) guarded only by a
/// JavaScript confirm(). The framework therefore NEVER navigates to a Remove URL except in the guarded
/// destructive test, and only for a row it created itself.
/// </summary>
public static class AdminModuleCatalog
{
    public static IReadOnlyList<AdminModule> All { get; } =
    [
        new("Sellings", "/administrator/sellings", "Sellings", ["Sellings"],
            Roles: "admin,manager", NavText: "Sellings", HasTable: true,
            Tabs: ["Moderation", "Active", "Finished without payment", "Finished", "Declined"],
            Notes: "Index, Edit (multipart form with photos/documents), AddPayment, RemoveBuyer, Remove. Only list/tab/edit-view are automated."),
        new("SellingOffers", "/administrator/sellingoffers", "Selling Offers", ["Selling Offers"],
            Roles: "admin,manager", NavText: "Selling offers", HasTable: true,
            Notes: "Index with status filter (?status=Active|Accepted|Declined), Details, Accept, Decline (POST; accept may capture payment)."),
        new("Auctions", "/administrator/auctions", "Auctions", ["Auctions"],
            Roles: "admin,manager", NavText: "Auctions", HasTable: true,
            Notes: "Index, Edit, ChangeWinner, AddPayment, Remove."),
        new("Moderation", "/administrator/moderation/next", "Moderate", [],
            Roles: "admin,manager", NavText: "New offers", HasTable: false,
            Notes: "Next -> first item awaiting moderation (SellingModeration/AuctionModeration) or Empty view. Approve/Decline are writes."),
        new("Dictionary", "/administrator/dictionary", "Dictionary", ["Dictionary", "Body types", "Colors", "Locations"],
            Roles: "admin", NavText: "Dictionaries", HasTable: true,
            AddForms:
            [
                new("/administrator/dictionary/editbodytype", "Edit body type", ["Name", "IsActive"]),
                new("/administrator/dictionary/editcolor", "Edit color", ["Name", "HexValue", "Exterior", "Interior"]),
                new("/administrator/dictionary/editlocation", "Edit location", ["Name"])
            ],
            Notes: "EditBodyType/EditColor/EditLocation and Remove* actions."),
        new("Manufacturers", "/administrator/manufacturers", "Manufacturers", ["Manufacturers"],
            Roles: "admin", NavText: "Manufacturers", HasTable: true,
            AddForms: [new("/administrator/manufacturers/edit", "Edit manufacturer", ["Name", "ImageFile", "PublishOnHomePage"])]),
        new("Models", "/administrator/models", "Models", ["Models"],
            Roles: "admin", NavText: "Models", HasTable: true,
            AddForms: [new("/administrator/models/edit", "Edit model", ["Name", "ManufacturerId"])]),
        new("Autodata", "/administrator/autodata", "Autodata Import", ["Autodata Import"],
            Roles: "admin", NavText: "Autodata import", HasTable: false,
            Notes: "Two import buttons (/Autodata/ImportTreeTables, /Autodata/ImportTree). Heavy production-data jobs: never clicked."),
        new("Banners", "/administrator/banners", "Banners", ["Banners"],
            Roles: "admin", NavText: "Banners", HasTable: true,
            Notes: "Index and Edit (images). No Add button in the view."),
        new("News", "/administrator/news", "Articles", ["Categories", "Articles"],
            Roles: "admin,manager", NavText: "News", HasTable: true,
            AddForms:
            [
                new("/administrator/news/edit", "Edit article", ["Title", "Alias", "Text", "PageDescription", "PageKeywords", "CategoryId", "ImageFile"]),
                new("/administrator/news/editcategory", "Edit category", ["Title", "PageTitle", "PageDescription"])
            ]),
        new("Pages", "/administrator/pages", "Pages", ["Pages"],
            Roles: "admin", NavText: "Pages", HasTable: true,
            AddForms: [new("/administrator/pages/edit", "Edit page", ["Alias", "Text", "Published", "PageTitle", "PageDescription", "PageKeywords"])]),
        new("Faq", "/administrator/faq", "FAQ", ["Categories", "Items"],
            Roles: "admin", NavText: "FAQ", HasTable: true,
            AddForms:
            [
                new("/administrator/faq/edit", "Edit item", ["CategoryId", "Question", "Answer", "ViewInShortList"]),
                new("/administrator/faq/editcategory", "Edit category", ["Name"])
            ],
            Notes: "Guarded destructive CRUD test creates, edits and removes an AUTO-marked FAQ category."),
        new("Clients", "/administrator/clients", "Clients", ["Clients"],
            Roles: "admin,manager", NavText: "Clients", HasTable: true,
            Notes: "Index, Edit (personal data), Remove. Edit view opened read-only only."),
        new("Administrators", "/administrator/administrators", "Administrators", ["Administrators", "Managers"],
            Roles: "admin", NavText: "Administrators", HasTable: true,
            AddForms:
            [
                new("/administrator/administrators/editadministrator", "Edit administrator", ["User.Login", "User.Email", "User.Phone", "User.NewPassword"]),
                new("/administrator/administrators/editmanager", "Edit manager", ["User.Login", "User.Email", "User.Phone", "User.NewPassword"])
            ],
            Notes: "Creating admin accounts from tests is out of scope (security)."),
        new("ServiceRequests", "/administrator/servicerequests", "Service requests",
            ["Finance requests", "Shipping requests", "VerifyBuy Inspection Report requests", "Car removal requests"],
            Roles: "admin", NavText: "Service requests", HasTable: true),
        new("TestDrive", "/administrator/testdrive", "Test Drive Requests", ["Test Drive Requests"],
            Roles: "admin,manager", NavText: "Test drives", HasTable: false,
            Tabs: ["Pending", "Approved", "Scheduled"],
            Notes: "Index (tabs), Details, UpdateDetails and status changes (POST)."),
        new("Payments", "/administrator/payments", "Payments", ["Payments"],
            Roles: "admin", NavText: "Payments", HasTable: true,
            ExpectedColumns: ["Id", "Date", "Status", "Purpose", "Method", "Sum"]),
        new("Comments", "/administrator/comments/moderation", "Comments", [],
            Roles: "admin,manager", NavText: "Comments", HasTable: false,
            Notes: "Moderation shows the next comment (form with Save and approve / Remove) or the Empty view."),
        // Controllers without a page of their own:
        new("Cars", "/administrator/cars", null, [], Roles: "admin,manager", NavText: null, HasTable: false, HasIndex: false,
            Notes: "Only RemovePhoto/RemoveDocument (GET, destructive) used from Sellings/Auctions edit pages. Not automated."),
        new("System", "/administrator/system", null, [], Roles: "admin,manager", NavText: null, HasTable: false, HasIndex: false,
            Notes: "Only UploadImage (TinyMCE image upload endpoint). Not a page; not automated.")
    ];

    public static IEnumerable<AdminModule> WithIndex => All.Where(m => m.HasIndex);
    public static IEnumerable<AdminModule> WithAddForms => All.Where(m => m.AddForms.Count > 0);
    public static IEnumerable<AdminModule> WithTabs => All.Where(m => m.Tabs.Count > 0);

    public static IEnumerable<AdminForm> AllAddForms => All.SelectMany(m => m.AddForms);

    public static AdminModule Get(string name) => All.Single(m => m.Name == name);
}

public sealed record AdminModule(
    string Name,
    string Path,
    string? ExpectedTitle,
    IReadOnlyList<string> ExpectedHeadings,
    string Roles,
    string? NavText,
    bool HasTable,
    IReadOnlyList<AdminForm>? AddForms = null,
    IReadOnlyList<string>? Tabs = null,
    IReadOnlyList<string>? ExpectedColumns = null,
    bool HasIndex = true,
    string? Notes = null)
{
    public IReadOnlyList<AdminForm> AddForms { get; init; } = AddForms ?? [];
    public IReadOnlyList<string> Tabs { get; init; } = Tabs ?? [];
    public IReadOnlyList<string> ExpectedColumns { get; init; } = ExpectedColumns ?? [];
    public bool AdminOnly => !Roles.Contains("manager");
    public override string ToString() => Name;
}

public sealed record AdminForm(string Path, string ExpectedHeading, IReadOnlyList<string> FieldNames)
{
    public override string ToString() => Path;
}
