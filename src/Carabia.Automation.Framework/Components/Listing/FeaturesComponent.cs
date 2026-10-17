using Carabia.Automation.Framework.Base;
using OpenQA.Selenium;

namespace Carabia.Automation.Framework.Components.Listing;

/// <summary>
/// Specifications (.lps-specs-grid .lps-spec-item: .lps-spec-label / .lps-spec-value) and the
/// feature icon grid (.lps-features-grid .lps-feature-item, hidden overflow items get "lps-hidden-item",
/// toggled by .lps-see-more-btn).
/// </summary>
public sealed class FeaturesComponent(BrowserSession session) : ComponentBase(session)
{
    private static readonly By SpecItems = By.CssSelector(".lps-specs-grid .lps-spec-item");
    private static readonly By FeatureGrids = By.CssSelector(".lps-features-grid.lps-js-grid-container");
    private static readonly By FeatureItem = By.CssSelector(".lps-feature-item");
    private static readonly By SeeMore = By.CssSelector(".lps-see-more-btn.lps-js-toggle-btn");

    public IReadOnlyDictionary<string, string> Specifications() =>
        Driver.FindElements(SpecItems)
            .Select(item => (
                Label: item.FindElements(By.CssSelector(".lps-spec-label")).FirstOrDefault()?.Text.Trim() ?? string.Empty,
                Value: item.FindElements(By.CssSelector(".lps-spec-value")).FirstOrDefault()?.Text.Trim() ?? string.Empty))
            .Where(s => s.Label.Length > 0)
            .GroupBy(s => s.Label, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First().Value, StringComparer.OrdinalIgnoreCase);

    public int SpecItemCount => Driver.FindElements(SpecItems).Count;

    public IReadOnlyList<string> SpecLabels() =>
        Driver.FindElements(SpecItems)
            .Select(item => item.FindElements(By.CssSelector(".lps-spec-label")).FirstOrDefault()?.Text.Trim() ?? string.Empty)
            .Where(t => t.Length > 0)
            .ToList();

    /// <summary>The first visible feature grid (desktop and mobile copies both exist in the markup).</summary>
    public IWebElement? VisibleFeatureGrid => Driver.FindElements(FeatureGrids).FirstOrDefault(g => g.Displayed);

    public int FeatureCount => VisibleFeatureGrid?.FindElements(FeatureItem).Count ?? 0;

    public int VisibleFeatureCount => VisibleFeatureGrid?.FindElements(FeatureItem).Count(e => e.Displayed) ?? 0;

    public IReadOnlyList<string> FeatureNames() =>
        VisibleFeatureGrid?.FindElements(FeatureItem).Select(e => (e.GetDomProperty("textContent") ?? string.Empty).Trim()).ToList() ?? [];

    public IReadOnlyList<IWebElement> FeatureIcons() =>
        VisibleFeatureGrid?.FindElements(By.CssSelector(".lps-feature-item img, .lps-feature-item svg")).ToList() ?? [];

    public IWebElement? SeeMoreButton =>
        VisibleFeatureGrid?.FindElement(By.XPath("..")).FindElements(SeeMore).FirstOrDefault(b => b.Displayed);

    public void ToggleSeeMore()
    {
        var button = SeeMoreButton ?? throw new Utilities.AutomationException("The features 'see more' button is not visible.");
        Step($"Features: click '{button.Text.Trim()}'", button);
        Elements.Click(button);
    }
}
