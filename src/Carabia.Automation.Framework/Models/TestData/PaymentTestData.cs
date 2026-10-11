namespace Carabia.Automation.Framework.Models.TestData;

/// <summary>
/// testdata/Payments/StripeTestData.json. Only Stripe's public TEST card numbers
/// (https://docs.stripe.com/testing) - they cannot charge real money and only work with pk_test_ keys.
/// </summary>
public sealed class PaymentTestData
{
    public string RequiredPublishableKeyPrefix { get; set; } = "pk_test_";
    public StripeTestCard SuccessCard { get; set; } = new();
    public StripeTestCard DeclinedCard { get; set; } = new();
}

public sealed class StripeTestCard
{
    public string Number { get; set; } = string.Empty;
    public string Expiry { get; set; } = "12/34";
    public string Cvc { get; set; } = "123";
    public string Description { get; set; } = string.Empty;
}
