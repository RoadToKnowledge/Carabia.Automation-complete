using Carabia.Automation.Framework.Components.Listing;
using Carabia.Automation.Framework.Flows;
using Carabia.Automation.Framework.Pages.Public;
using Carabia.Automation.Tests.Base;

namespace Carabia.Automation.Tests.Listings;

/// <summary>Dynamic feature icons (.lps-features-grid) and the Leaflet location map with its zoom widget.</summary>
[TestFixture]
[Category(TestCategories.Public)]
[Category(TestCategories.Listings)]
[Category(TestCategories.Regression)]
[Parallelizable(ParallelScope.Self)]
public sealed class FeatureAndLocationTests : UiTestBase
{
    private ListingDetailsPage OpenListing()
    {
        new BrowseCarsFlow(Session).OpenListingFromResults();
        return new ListingDetailsPage(Session);
    }

    [Test]
    public void Feature_Items_Have_Names_And_Icons()
    {
        var page = OpenListing();
        if (page.Features.FeatureCount == 0) Assert.Inconclusive("The first listing has no features.");
        Assert.Multiple(() =>
        {
            Assert.That(page.Features.FeatureNames().All(n => n.Length > 0), Is.True, "A feature item has no text.");
            Assert.That(page.Features.FeatureIcons(), Is.Not.Empty, "Feature items have no icons.");
        });
    }

    [Test]
    public void See_More_Reveals_Hidden_Features()
    {
        var page = OpenListing();
        if (page.Features.SeeMoreButton is null) Assert.Inconclusive("Not enough features for a 'see more' button.");
        var visibleBefore = page.Features.VisibleFeatureCount;
        page.Features.ToggleSeeMore();
        Session.WaitFor("more features to become visible", _ => page.Features.VisibleFeatureCount > visibleBefore);
        Assert.That(page.Features.VisibleFeatureCount, Is.EqualTo(page.Features.FeatureCount));
    }

    [Test]
    public void Location_Map_Renders_With_Header()
    {
        var page = OpenListing();
        Assert.That(page.Map.IsPresent, Is.True, "#lpsLeafletMap is missing.");
        page.Map.ScrollTo();
        Assert.Multiple(() =>
        {
            Assert.That(page.Map.HeaderText, Does.StartWith("Location"));
            Assert.That(page.Map.WaitForTiles(), Is.True, "Leaflet map tiles did not load.");
        });
    }

    [Test]
    public void Map_Zoom_Buttons_Step_Through_Levels()
    {
        var page = OpenListing();
        page.Map.ScrollTo();
        Assert.That(page.Map.ZoomText, Is.EqualTo("100%"));
        Assert.That(page.Map.ClickZoomIn(), Is.EqualTo("120%"));
        Assert.That(page.Map.ClickZoomOut(), Is.EqualTo("100%"));
        Assert.That(page.Map.ClickZoomOut(), Is.EqualTo("80%"));
        Assert.That(LocationMapComponent.ZoomLevels, Does.Contain(page.Map.ZoomText));
    }
}
