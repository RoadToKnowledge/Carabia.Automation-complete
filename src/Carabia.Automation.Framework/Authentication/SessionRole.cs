namespace Carabia.Automation.Framework.Authentication;

/// <summary>
/// Carabia has two kinds of login:
///  - client (role "client"): phone + OTP popup. Buyer and Seller are both clients; use two different
///    test accounts so offers between them are possible.
///  - administrator / manager: username + password at /Identity/Authorization.
/// </summary>
public enum SessionRole
{
    Public,
    Buyer,
    Seller,
    Administrator
}

public static class SessionRoleExtensions
{
    public static bool IsClient(this SessionRole role) => role is SessionRole.Buyer or SessionRole.Seller;
    public static string Upper(this SessionRole role) => role.ToString().ToUpperInvariant();
}
