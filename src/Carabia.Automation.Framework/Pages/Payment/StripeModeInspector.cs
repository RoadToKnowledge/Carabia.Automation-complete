using OpenQA.Selenium;

namespace Carabia.Automation.Framework.Pages.Payment;

/// <summary>
/// Proves whether a page would talk to Stripe in TEST or LIVE mode, by reading the prefix of
/// window.StripePublishableKey (set by PaymentDetails.cshtml and Sell.cshtml). The key itself is never logged.
/// Payment write tests may only run when this returns "test".
/// </summary>
public static class StripeModeInspector
{
    public static string Mode(IWebDriver driver)
    {
        var prefix = ((IJavaScriptExecutor)driver).ExecuteScript(
            "const k = window.StripePublishableKey || ''; return k.substring(0, 8);") as string ?? string.Empty;
        if (prefix.StartsWith("pk_test_", StringComparison.Ordinal)) return "test";
        if (prefix.StartsWith("pk_live_", StringComparison.Ordinal)) return "live";
        return prefix.Length == 0 ? "missing" : "unknown";
    }
}
