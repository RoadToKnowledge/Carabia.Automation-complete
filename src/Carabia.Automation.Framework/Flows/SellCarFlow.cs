using Carabia.Automation.Framework.Base;
using Carabia.Automation.Framework.Components.Sell;
using Carabia.Automation.Framework.Models.Results;
using Carabia.Automation.Framework.Models.TestData;
using Carabia.Automation.Framework.Pages.Sell;
using Carabia.Automation.Framework.Utilities;

namespace Carabia.Automation.Framework.Flows;

/// <summary>Sell My Car end-to-end steps for both vehicle paths (VIN and manual), ported from carabia-ui-tests.</summary>
public sealed class SellCarFlow(BrowserSession session) : FlowBase(session)
{
    public SellCarPage Page { get; } = new(session);

    /// <summary>Step 1 -> 2 and checks that the identified vehicle matches the test data.</summary>
    public void IdentifyVehicle(VehicleTestData vehicle, bool manual)
    {
        Page.Open();
        if (manual)
        {
            Page.Vehicle.SelectManually(vehicle);
            if (Page.Wizard.Value(VehicleSelectorComponent.VinInput).Length != 0)
                throw new AutomationException("The VIN field should stay empty on the manual path.");
            Page.Vehicle.SubmitManual();
        }
        else
        {
            Page.Vehicle.LookupByVin(vehicle.Vin);
        }

        var header = Page.Vehicle.VehicleCardHeaderText;
        foreach (var expected in new[] { vehicle.Make, vehicle.Model, vehicle.Trim, vehicle.Year })
            if (!header.Contains(expected, StringComparison.OrdinalIgnoreCase))
                throw new AutomationException($"Step 2 vehicle card '{header}' does not mention '{expected}'.");
        if (string.IsNullOrWhiteSpace(Page.Wizard.Value("sellCarAdmeId")))
            throw new AutomationException("Vehicle lookup did not provide an ADME vehicle id (#sellCarAdmeId).");
        if (!manual && Page.Wizard.Value("sellCarDecodedVin") != vehicle.Vin)
            throw new AutomationException($"Decoded VIN is '{Page.Wizard.Value("sellCarDecodedVin")}', expected {vehicle.Vin}.");
        Step($"PASS: {(manual ? "manual search" : "VIN lookup")} identified {vehicle.FullName}");
    }

    /// <summary>Steps 2 and 3: details, location, owner notes; ends on step 4.</summary>
    public string CompleteDetailsAndFeatures(VehicleTestData vehicle, string marker)
    {
        Page.FillVehicleDetails(vehicle);
        Page.ContinueFromDetails();
        var notes = vehicle.OwnerNotesTemplate.Replace("{marker}", marker);
        Page.AddOwnerNotes(notes);
        var saved = Page.ReadSavedOwnerNotes();
        if (saved != notes)
            throw new AutomationException($"Owner notes were saved as '{saved}', expected '{notes}'.");
        return notes;
    }

    /// <summary>Steps 4-9 with free options only; ends on the summary without submitting.</summary>
    public void CompleteToSummary(VehicleTestData vehicle, IReadOnlyCollection<string> photos)
    {
        Page.ContinueFromFeatures();
        Page.Price.EnterPrice(vehicle.AskingPrice);
        Page.Price.Continue();
        Page.Photos.Upload(photos);
        Page.Photos.WaitForLoadedPreviews(photos.Count);
        Page.Photos.ClickNext();
        Page.Wizard.WaitForStep(6);
        Page.SkipOwnershipVerification();
        Page.ChooseFreePlanAndNoBoost();
    }

    /// <summary>Steps 4-7 (price, photos, ownership skipped, free plan); ends on the boost step (8).</summary>
    public void CompleteToBoostStep(VehicleTestData vehicle, IReadOnlyCollection<string> photos)
    {
        Page.ContinueFromFeatures();
        Page.Price.EnterPrice(vehicle.AskingPrice);
        Page.Price.Continue();
        Page.Photos.Upload(photos);
        Page.Photos.WaitForLoadedPreviews(photos.Count);
        Page.Photos.ClickNext();
        Page.Wizard.WaitForStep(6);
        Page.SkipOwnershipVerification();
        Page.SkipToBoostStep();
    }

    public SellListingResult ReadSummary(string marker) => Page.ReadSummary(marker);

    public static string[] PhotoFiles(Configuration.TestSettings settings)
    {
        var overrideDirectory = Environment.GetEnvironmentVariable("CARABIA_SELL_PHOTO_DIR");
        var directory = !string.IsNullOrWhiteSpace(overrideDirectory)
            ? overrideDirectory
            : TestDataReader.FilePath(settings, "Uploads", "photos");
        if (!Directory.Exists(directory))
            throw new AutomationException($"Photo folder '{directory}' is missing. Set CARABIA_SELL_PHOTO_DIR or restore testdata/Uploads/photos.");
        var photos = Directory.GetFiles(directory)
            .Where(p => new[] { ".jpg", ".jpeg", ".jfif", ".png", ".webp" }.Contains(Path.GetExtension(p).ToLowerInvariant()))
            .OrderBy(p => p, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        if (photos.Length < ImageUploadComponent.MinimumPhotos)
            throw new AutomationException($"Sell My Car needs at least {ImageUploadComponent.MinimumPhotos} photos; found {photos.Length} in '{directory}'.");
        return photos;
    }
}
