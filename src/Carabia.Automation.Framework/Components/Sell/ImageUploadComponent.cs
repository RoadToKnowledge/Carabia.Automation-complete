using Carabia.Automation.Framework.Base;
using Carabia.Automation.Framework.Utilities;
using OpenQA.Selenium;

namespace Carabia.Automation.Framework.Components.Sell;

/// <summary>
/// Step 5 photos (PUG uploader in Sell.cshtml): hidden file input #pug-file-input (accept jpg/jpeg/jfif/png/webp),
/// previews "#pug-grid .pug-cell > img", cover ".pug-is-cover", error #pug-photo-error
/// ("Please upload at least 4 photos before continuing."), next #sellCarPhotosNext. Minimum is PUG_MIN_PHOTOS = 4.
/// Photos stay as local previews until the listing is submitted.
/// </summary>
public sealed class ImageUploadComponent(BrowserSession session) : ComponentBase(session)
{
    public const int MinimumPhotos = 4;
    public const string MinimumPhotosMessage = "at least 4 photos";

    private static readonly By FileInput = By.Id("pug-file-input");
    private static readonly By Previews = By.CssSelector("#pug-grid .pug-cell > img");
    private static readonly By Cells = By.CssSelector("#pug-grid .pug-cell");
    private static readonly By Cover = By.CssSelector("#pug-grid .pug-is-cover > img");
    private static readonly By Error = By.Id("pug-photo-error");
    private const string NextId = "sellCarPhotosNext";

    private SellWizardComponent Wizard { get; } = new(session);

    public string AcceptAttribute => Driver.FindElement(FileInput).GetDomAttribute("accept") ?? string.Empty;
    public int CellCount => Driver.FindElements(Cells).Count;
    public string? CoverSource => Driver.FindElements(Cover).FirstOrDefault()?.GetDomProperty("src");

    public void ClickNext() => Wizard.ClickById(NextId, "Sell: photos Next");

    public string WaitForError() => Session.Visible(Error, "the photo validation message").Text.Trim();
    public bool ErrorVisible => Driver.FindElement(Error).Displayed;

    /// <summary>Sends files straight to the hidden input (Selenium bypasses the OS dialog and its accept filter).</summary>
    public void Upload(IReadOnlyCollection<string> files)
    {
        var missing = files.Where(f => !File.Exists(f)).ToList();
        if (missing.Count > 0)
            throw new AutomationException($"Upload files not found: {string.Join(", ", missing)}");
        Step($"Sell: upload {files.Count} file(s)");
        Driver.FindElement(FileInput).SendKeys(string.Join("\n", files.Select(Path.GetFullPath)));
    }

    public void WaitForLoadedPreviews(int expected) =>
        Session.WaitFor($"{expected} photo previews to load", d =>
        {
            var previews = d.FindElements(Previews);
            return previews.Count == expected && previews.All(img => JavaScriptHelper.IsImageLoaded(d, img));
        }, Session.LongTimeout);

    /// <summary>Number of preview images that the browser could not decode (e.g. a .txt file accepted by mistake).</summary>
    public int BrokenPreviewCount() =>
        Driver.FindElements(Previews).Count(img => !JavaScriptHelper.IsImageLoaded(Driver, img));
}
