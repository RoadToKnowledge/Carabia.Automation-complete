namespace Carabia.Automation.Framework.Models.TestData;

/// <summary>testdata/Vehicles/SellVehicles.json - vehicles used by the Sell My Car wizard.</summary>
public sealed class SellVehiclesFile
{
    public VehicleTestData Default { get; set; } = new();
    public List<string> InvalidVins { get; set; } = [];
}

/// <summary>
/// One vehicle that is known to the Autodata lookup service. The default (VIN 2FMPK4G83GBB65542,
/// 2016 Ford Edge SE, GCC) is the vehicle verified by carabia-ui-tests.
/// </summary>
public sealed class VehicleTestData
{
    public string Vin { get; set; } = string.Empty;
    public string Year { get; set; } = string.Empty;
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string Trim { get; set; } = string.Empty;
    public string RegionalSpec { get; set; } = "GCC";
    public string Mileage { get; set; } = "80000";
    public string ExteriorColor { get; set; } = "White";
    public string InteriorColor { get; set; } = "Black";
    public string Warranty { get; set; } = "No";
    public LocationTestData Location { get; set; } = new();
    public string AskingPrice { get; set; } = "40000";

    /// <summary>Owner notes; "{marker}" is replaced by an AUTO-... marker so the listing is identifiable.</summary>
    public string OwnerNotesTemplate { get; set; } = "UI automation sample {marker}";

    public string FullName => $"{Year} {Make} {Model} {Trim}";
}
