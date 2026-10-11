using OpenQA.Selenium;

namespace Carabia.Automation.Framework.Authentication;

/// <summary>Converts between Selenium cookies and the JSON snapshot.</summary>
public static class CookieStorage
{
    /// <summary>Carabia's cookie authentication scheme uses this cookie name (Program.cs).</summary>
    public const string AuthCookieName = "Carabia.Auth";

    public static List<StoredCookie> Capture(IWebDriver driver) =>
        driver.Manage().Cookies.AllCookies.Select(cookie => new StoredCookie
        {
            Name = cookie.Name,
            Value = cookie.Value,
            Domain = cookie.Domain,
            Path = cookie.Path ?? "/",
            Expiry = cookie.Expiry,
            Secure = cookie.Secure,
            HttpOnly = cookie.IsHttpOnly,
            SameSite = cookie.SameSite
        }).ToList();

    /// <summary>The browser must already be on the Carabia origin, otherwise cookies are rejected.</summary>
    public static int Restore(IWebDriver driver, IEnumerable<StoredCookie> cookies)
    {
        var restored = 0;
        foreach (var stored in cookies)
        {
            if (stored.Expiry is { } expiry && expiry.ToUniversalTime() <= DateTime.UtcNow)
                continue;
            try
            {
                driver.Manage().Cookies.AddCookie(new Cookie(stored.Name, stored.Value, stored.Domain, stored.Path,
                    stored.Expiry, stored.Secure, stored.HttpOnly, stored.SameSite));
                restored++;
            }
            catch (Exception exception) when (exception is WebDriverException or ArgumentException)
            {
                // A cookie for another domain (analytics etc.) cannot be set here; it is not needed.
            }
        }
        return restored;
    }

    public static bool HasUnexpiredAuthCookie(SessionSnapshot snapshot) =>
        snapshot.Cookies.Any(c => c.Name == AuthCookieName && (c.Expiry is null || c.Expiry.Value.ToUniversalTime() > DateTime.UtcNow));
}
