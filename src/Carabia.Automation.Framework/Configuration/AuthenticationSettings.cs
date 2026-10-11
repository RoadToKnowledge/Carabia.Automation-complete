namespace Carabia.Automation.Framework.Configuration;

/// <summary>
/// Values from the "Authentication" section. These only describe WHERE the person logs in
/// and WHERE the test continues afterwards. No credentials, phone numbers or OTPs are stored.
/// </summary>
public sealed class AuthenticationSettings
{
    /// <summary>Client (buyer/seller) login is the phone/OTP popup opened from the home page "Sign in" button.</summary>
    public string ClientLoginPath { get; set; } = "/";

    /// <summary>Administrator/manager login is the username/password form of IdentityController.Authorization.</summary>
    public string AdministratorLoginPath { get; set; } = "/Identity/Authorization";

    public string BuyerStartPath { get; set; } = "/personal/buying";
    public string SellerStartPath { get; set; } = "/personal/selling";
    public string AdministratorStartPath { get; set; } = "/administrator/sellings";

    /// <summary>
    /// Optional convenience: country code pre-selected in the client login popup (for example "+91" or "+971").
    /// Leave empty to select it yourself. The phone number and OTP are always typed by you.
    /// </summary>
    public string PreselectCountryCode { get; set; } = "+91";

    /// <summary>A saved session older than this is ignored and you are asked to log in again.</summary>
    public int SessionMaxAgeHours { get; set; } = 24 * 20;
}
