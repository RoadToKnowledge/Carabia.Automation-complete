using Carabia.Automation.Framework.Base;
using OpenQA.Selenium;

namespace Carabia.Automation.Framework.Components.Listing;

/// <summary>
/// Listing location module: .lps-map-header ("Location: ..."), Leaflet map #lpsLeafletMap,
/// zoom widget #lpsZoomIn / #lpsZoomOut / #lpsZoomLabel with levels 60%..140% (default 100%).
/// </summary>
public sealed class LocationMapComponent(BrowserSession session) : ComponentBase(session)
{
    public static readonly string[] ZoomLevels = ["60%", "80%", "100%", "120%", "140%"];

    private static readonly By Header = By.CssSelector(".lps-map-module .lps-map-header");
    private static readonly By Map = By.Id("lpsLeafletMap");
    private static readonly By MapTiles = By.CssSelector("#lpsLeafletMap .leaflet-tile-loaded, #lpsLeafletMap .leaflet-tile");
    private static readonly By ZoomIn = By.Id("lpsZoomIn");
    private static readonly By ZoomOut = By.Id("lpsZoomOut");
    private static readonly By ZoomLabel = By.Id("lpsZoomLabel");

    public bool IsPresent => Elements.Exists(Map);
    public string HeaderText => Driver.FindElements(Header).FirstOrDefault()?.Text.Trim() ?? string.Empty;
    public string ZoomText => Driver.FindElement(ZoomLabel).Text.Trim();

    public void ScrollTo() => Step("Map: check listing location", Driver.FindElement(Map));

    public bool WaitForTiles() =>
        Utilities.WaitHelper.TryUntil(Driver, d => d.FindElements(MapTiles).Count > 0, Session.LongTimeout);

    public string ClickZoomIn()
    {
        var button = Elements.Clickable(ZoomIn, "the map zoom-in button");
        Elements.Click(button);
        return ZoomText;
    }

    public string ClickZoomOut()
    {
        var button = Elements.Clickable(ZoomOut, "the map zoom-out button");
        Elements.Click(button);
        return ZoomText;
    }
}
