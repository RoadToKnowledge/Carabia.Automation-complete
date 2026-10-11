namespace Carabia.Automation.Framework.Models.Results;

/// <summary>What the sell wizard showed on its summary (step 9) and, if submitted, the success screen (step 11).</summary>
public sealed class SellListingResult
{
    public string Marker { get; init; } = string.Empty;
    public string SummaryCarName { get; init; } = string.Empty;
    public string SummaryMeta { get; init; } = string.Empty;
    public decimal SummaryAskingPrice { get; init; }
    public string SummaryColours { get; init; } = string.Empty;
    public string SummaryLocation { get; init; } = string.Empty;
    public string SummaryOwnerNotes { get; init; } = string.Empty;
    public string SummaryPhotoCount { get; init; } = string.Empty;
    public decimal SummaryPaymentTotal { get; init; }
    public bool Submitted { get; init; }
    public string? SuccessTitle { get; init; }
}

/// <summary>Result of an offer action on the offer page or in My Garage.</summary>
public sealed class OfferResult
{
    public int SellingId { get; init; }
    public decimal Amount { get; init; }
    public bool Submitted { get; init; }
    public string Message { get; init; } = string.Empty;
}

/// <summary>Result of reading a payment page (never charges anything).</summary>
public sealed class PaymentResult
{
    public string PublishableKeyMode { get; init; } = "unknown";
    public bool IsStripeTestMode => PublishableKeyMode == "test";
    public string Notes { get; init; } = string.Empty;
}
