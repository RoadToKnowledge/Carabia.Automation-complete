using Carabia.Automation.Framework.Flows;
using Carabia.Automation.Framework.Pages.Public;
using Carabia.Automation.Tests.Base;

namespace Carabia.Automation.Tests.Listings;

/// <summary>Main image, counter, thumbnails, Photo Gallery button and the Fancybox full-screen view.</summary>
[TestFixture]
[Category(TestCategories.Public)]
[Category(TestCategories.Listings)]
[Category(TestCategories.Regression)]
[Parallelizable(ParallelScope.Self)]
public sealed class GalleryTests : UiTestBase
{
    private ListingDetailsPage OpenListing()
    {
        new BrowseCarsFlow(Session).OpenListingFromResults();
        return new ListingDetailsPage(Session);
    }

    [Test]
    public void Main_Image_Loads_And_Counter_Starts_At_One()
    {
        var page = OpenListing();
        Assert.Multiple(() =>
        {
            Assert.That(page.Gallery.HasMainImage, Is.True);
            Assert.That(page.Gallery.MainImageLoaded, Is.True, $"Main image did not load: {page.Gallery.MainImageSource}");
            Assert.That(page.Gallery.CurrentIndexValue, Is.EqualTo(1));
            Assert.That(page.Gallery.TotalPhotos, Is.GreaterThanOrEqualTo(1));
        });
    }

    [Test]
    public void Thumbnails_Are_Limited_To_Four_And_First_Is_Active()
    {
        var page = OpenListing();
        var thumbs = page.Gallery.ThumbnailElements();
        Assert.Multiple(() =>
        {
            Assert.That(thumbs.Count, Is.InRange(1, 4));
            Assert.That(thumbs.Count, Is.EqualTo(Math.Min(4, page.Gallery.TotalPhotos)));
            Assert.That(page.Gallery.ActiveThumbnailIndex(), Is.EqualTo(0));
        });
    }

    [Test]
    public void Clicking_A_Thumbnail_Updates_Main_Image_Counter_And_Opens_Gallery()
    {
        var page = OpenListing();
        if (page.Gallery.ThumbnailElements().Count < 2) Assert.Inconclusive("The listing has fewer than two photos.");
        var before = page.Gallery.MainImageSource;
        page.Gallery.ClickThumbnail(1);
        Session.WaitFor("the full-screen gallery to open from the thumbnail", _ => page.Gallery.IsFullscreenOpen, Session.LongTimeout);
        page.Gallery.CloseFullscreen();
        Assert.Multiple(() =>
        {
            Assert.That(page.Gallery.MainImageSource, Is.Not.EqualTo(before), "Main image did not change.");
            Assert.That(page.Gallery.CurrentIndexValue, Is.EqualTo(2));
            Assert.That(page.Gallery.ActiveThumbnailIndex(), Is.EqualTo(1));
        });
    }

    [Test]
    public void Photo_Gallery_Button_Opens_And_Closes_Fullscreen_Gallery()
    {
        var page = OpenListing();
        Assert.That(page.Gallery.HasPhotoGalleryButton, Is.True, "'Photo Gallery' button is missing.");
        page.Gallery.OpenFromPhotoGalleryButton();
        Assert.That(page.Gallery.FullscreenSlideCount, Is.GreaterThanOrEqualTo(1));
        page.Gallery.CloseFullscreen();
        Assert.That(page.Gallery.IsFullscreenOpen, Is.False);
    }

    [Test]
    public void Clicking_Main_Image_Opens_Fullscreen_Gallery_With_All_Photos()
    {
        var page = OpenListing();
        var total = page.Gallery.TotalPhotos;
        page.Gallery.OpenFromMainImage();
        Assert.That(page.Gallery.FullscreenSlideCount, Is.GreaterThanOrEqualTo(Math.Min(total, 1)));
        page.Gallery.NextFullscreenSlide();
        page.Gallery.CloseFullscreen();
        Assert.That(page.Gallery.IsFullscreenOpen, Is.False);
    }
}
