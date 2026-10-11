namespace Carabia.Automation.Framework.Models.TestData;

/// <summary>testdata/Offers/OfferData.json</summary>
public sealed class OfferTestData
{
    /// <summary>Offer = asking price x this ratio (the page enforces a minimum of 75%, see payment.js / data-min-offer).</summary>
    public decimal ValidOfferRatio { get; set; } = 0.9m;

    /// <summary>Below the 75% minimum; must be rejected client-side.</summary>
    public decimal TooLowOfferRatio { get; set; } = 0.5m;

    public decimal CounterOfferRatio { get; set; } = 0.95m;

    /// <summary>
    /// Listing id used by WRITE offer tests. MUST be a listing created by automation (seller test account),
    /// never a real customer's car. Empty = offer write tests are skipped.
    /// </summary>
    public int? AutomationSellingId { get; set; }
}
