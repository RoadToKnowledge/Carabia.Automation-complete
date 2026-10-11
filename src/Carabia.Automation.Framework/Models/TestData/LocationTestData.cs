namespace Carabia.Automation.Framework.Models.TestData;

/// <summary>A location typed into the Google Places field of the sell wizard and the values it must resolve to.</summary>
public sealed class LocationTestData
{
    public string SearchText { get; set; } = "Dubai";
    public string SuggestionContains { get; set; } = "Dubai";
    public string ExpectedCity { get; set; } = "Dubai";
    public string ExpectedCountryCode { get; set; } = "AE";
}
