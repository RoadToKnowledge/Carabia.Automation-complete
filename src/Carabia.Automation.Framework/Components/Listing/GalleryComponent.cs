using Carabia.Automation.Framework.Base;
using Carabia.Automation.Framework.Utilities;
using OpenQA.Selenium;

namespace Carabia.Automation.Framework.Components.Listing;

/// <summary>
/// Listing photo gallery (Views/Sellings/Selling.cshtml):
///   main image #mainImage inside #lpsMainGalleryImage (click -> openSellingGallery),
///   counter #current-index, thumbnails .lps-thumbnails .lps-thumb-wrapper (active: lps-active),
///   "Photo Gallery" button .lps-btn-gallery. At most 4 thumbnails are rendered; clicking one swaps the main
///   image AND opens the full-screen gallery at that photo (openThumbnailGallery). The "+N more" overlay is
///   commented out in the current source, so it is not automated.
/// Full-screen view is Fancybox 4 (.fancybox__container).
/// </summary>
public sealed class GalleryComponent(BrowserSession session) : ComponentBase(session)
{
    private static readonly By MainImageWrapper = By.Id("lpsMainGalleryImage");
    private static readonly By MainImage = By.Id("mainImage");
    private static readonly By CurrentIndex = By.Id("current-index");
    private static readonly By Thumbnails = By.CssSelector(".lps-thumbnails .lps-thumb-wrapper");
    private static readonly By PhotoGalleryButton = By.CssSelector(".lps-btn-gallery");
    private static readonly By FullscreenContainer = By.CssSelector(".fancybox__container");
    private static readonly By FullscreenSlides = By.CssSelector(".fancybox__container .fancybox__slide");
    private static readonly By FullscreenClose = By.CssSelector(".fancybox__container [data-fancybox-close], .fancybox__container .carousel__button.is-close");

    public bool HasMainImage => Elements.IsDisplayed(MainImage);
    public string MainImageSource => Driver.FindElement(MainImage).GetDomProperty("src") ?? string.Empty;
    public bool MainImageLoaded => JavaScriptHelper.IsImageLoaded(Driver, Driver.FindElement(MainImage));
    public string CounterText => Driver.FindElements(CurrentIndex).FirstOrDefault()?.FindElement(By.XPath("..")).Text.Trim() ?? string.Empty;
    public int CurrentIndexValue => int.TryParse(Driver.FindElement(CurrentIndex).Text.Trim(), out var i) ? i : 0;

    /// <summary>Total photo count from the counter "1/12".</summary>
    public int TotalPhotos => int.TryParse(CounterText.Split('/').LastOrDefault()?.Trim(), out var n) ? n : 0;

    public IReadOnlyList<IWebElement> ThumbnailElements() => Driver.FindElements(Thumbnails);
    public bool HasPhotoGalleryButton => Elements.IsDisplayed(PhotoGalleryButton);

    public int ActiveThumbnailIndex()
    {
        var thumbs = ThumbnailElements();
        for (var i = 0; i < thumbs.Count; i++)
            if ((thumbs[i].GetDomAttribute("class") ?? string.Empty).Contains("lps-active"))
                return i;
        return -1;
    }

    public bool IsFullscreenOpen => Elements.IsDisplayed(FullscreenContainer);
    public int FullscreenSlideCount => Driver.FindElements(FullscreenSlides).Count;

    public void OpenFromPhotoGalleryButton()
    {
        var button = Elements.Clickable(PhotoGalleryButton, "the 'Photo Gallery' button");
        Step("Gallery: open via Photo Gallery", button);
        Elements.Click(button);
        Session.WaitFor("the full-screen gallery (Fancybox) to open", _ => IsFullscreenOpen, Session.LongTimeout);
    }

    public void OpenFromMainImage()
    {
        var image = Elements.Clickable(MainImageWrapper, "the main listing image");
        Step("Gallery: open by clicking the main image", image);
        Elements.Click(image);
        Session.WaitFor("the full-screen gallery to open from the main image", _ => IsFullscreenOpen, Session.LongTimeout);
    }

    /// <summary>Clicking a thumbnail swaps it into the main image and opens the full-screen gallery at that photo.</summary>
    public void ClickThumbnail(int index)
    {
        var thumb = ThumbnailElements()[index];
        Step($"Gallery: click thumbnail {index + 1}", thumb);
        Elements.Click(thumb);
    }

    public void CloseFullscreen()
    {
        Step("Gallery: close full-screen view");
        var close = Driver.FindElements(FullscreenClose).FirstOrDefault(e => e.Displayed);
        if (close is not null)
            Elements.Click(close);
        else
            new OpenQA.Selenium.Interactions.Actions(Driver).SendKeys(Keys.Escape).Perform();
        Session.WaitFor("the full-screen gallery to close", _ => !IsFullscreenOpen);
    }

    public void NextFullscreenSlide() =>
        new OpenQA.Selenium.Interactions.Actions(Driver).SendKeys(Keys.ArrowRight).Perform();
}
