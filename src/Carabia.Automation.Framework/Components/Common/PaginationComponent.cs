using Carabia.Automation.Framework.Base;
using OpenQA.Selenium;

namespace Carabia.Automation.Framework.Components.Common;

/// <summary>Views/Sellings/Partial/_IndexListingPartial.cshtml (.product-grid__pagination). Page links go to {baseUrl}/page-N.</summary>
public sealed class PaginationComponent(BrowserSession session) : ComponentBase(session)
{
    private static readonly By Root = By.CssSelector(".crb-listing-results .product-grid__pagination, .product-grid__pagination");
    private static readonly By ActiveLabel = By.CssSelector(".product-grid__pagination .pagination__label_state_active");
    private static readonly By PageLinks = By.CssSelector(".product-grid__pagination a.pagination__link.js-pagination-link");

    public bool IsPresent => Elements.Exists(Root);

    public int? ActivePage =>
        Driver.FindElements(ActiveLabel).FirstOrDefault() is { } label && int.TryParse(label.Text.Trim(), out var page) ? page : null;

    public IReadOnlyList<(int Page, string Href)> Links() =>
        Driver.FindElements(PageLinks)
            .Select(a => (Page: int.TryParse(a.GetDomAttribute("data-page"), out var p) ? p : 0, Href: a.GetDomProperty("href") ?? string.Empty))
            .Where(l => l.Page > 0)
            .ToList();

    public void ClickPage(int page)
    {
        var link = Driver.FindElements(PageLinks).FirstOrDefault(a => a.GetDomAttribute("data-page") == page.ToString())
                   ?? throw new Utilities.AutomationException($"Pagination has no link to page {page}.");
        Step($"Pagination: open page {page}", link);
        Elements.Click(link);
    }
}
