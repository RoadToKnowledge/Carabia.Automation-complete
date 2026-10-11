using OpenQA.Selenium;

namespace Carabia.Automation.Framework.Authentication;

/// <summary>
/// Reads IdentityController.AuthStatus (GET /Identity/AuthStatus) from inside the browser, so the
/// browser's own cookies are used: { isAuthenticated, isClient, name }.
/// This is the same signal the working carabia-ui-tests use to detect a completed manual login.
/// </summary>
public static class AuthStatusProbe
{
    public static AuthStatus Read(IWebDriver driver)
    {
        try
        {
            var result = ((IJavaScriptExecutor)driver).ExecuteAsyncScript("""
                const done = arguments[arguments.length - 1];
                fetch('/Identity/AuthStatus', { credentials: 'same-origin', cache: 'no-store', headers: { 'X-Requested-With': 'XMLHttpRequest' } })
                    .then(r => r.ok ? r.json() : null)
                    .then(s => done(s ? [s.isAuthenticated === true, s.isClient === true, s.name || ''] : null))
                    .catch(() => done(null));
                """);

            if (result is IReadOnlyList<object> values && values.Count == 3)
                return new AuthStatus(values[0] is true, values[1] is true, values[2]?.ToString(), true);
        }
        catch (WebDriverException)
        {
            // Navigation in progress (for example while the OTP form redirects). Treat as unknown.
        }

        return new AuthStatus(false, false, null, false);
    }
}

public sealed record AuthStatus(bool IsAuthenticated, bool IsClient, string? Name, bool Reachable)
{
    public bool IsAnonymous => Reachable && !IsAuthenticated;
}
