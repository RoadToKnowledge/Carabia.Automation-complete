using Carabia.Automation.Framework.Utilities;
using OpenQA.Selenium;

namespace Carabia.Automation.Framework.Authentication;

/// <summary>Decides whether the current browser is authenticated for a given role.</summary>
public static class SessionValidator
{
    private static readonly By AdminLeftNavigation = By.CssSelector("#navbarNav .nav-link");

    public static bool IsAuthenticatedFor(IWebDriver driver, SessionRole role)
    {
        var status = AuthStatusProbe.Read(driver);
        return role switch
        {
            SessionRole.Public => true,
            SessionRole.Buyer or SessionRole.Seller => status.IsAuthenticated && status.IsClient,
            // Admins and managers are authenticated but not in the "client" role.
            SessionRole.Administrator => status.IsAuthenticated && !status.IsClient,
            _ => false
        };
    }

    /// <summary>
    /// Stronger administrator check: the Administrator area must actually render (cookie auth redirects
    /// unauthorised users to "/").
    /// </summary>
    public static bool AdministratorAreaOpens(IWebDriver driver, string baseUrl, string adminStartPath, TimeSpan timeout)
    {
        driver.Navigate().GoToUrl(UrlHelper.Combine(baseUrl, adminStartPath));
        return WaitHelper.TryUntil(driver, d =>
            UrlHelper.PathOf(d.Url).StartsWith("/administrator", StringComparison.OrdinalIgnoreCase)
            && d.FindElements(AdminLeftNavigation).Count > 0, timeout);
    }

    public static bool WaitUntilAuthenticated(IWebDriver driver, SessionRole role, TimeSpan timeout) =>
        WaitHelper.TryUntil(driver, d => IsAuthenticatedFor(d, role), timeout);
}
