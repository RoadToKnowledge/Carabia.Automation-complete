namespace Carabia.Automation.Framework.Configuration;

/// <summary>
/// Central fail-safe for anything that writes data. Production can never run write tests,
/// even if a config file or environment variable tries to switch them on.
/// </summary>
public static class SafetyGuard
{
    public static void Apply(TestSettings settings)
    {
        if (!IsProduction(settings))
            return;

        var execution = settings.Execution;
        execution.Environment = EnvironmentType.Prod;
        execution.AllowDestructiveTests = false;
        execution.AllowPaymentWriteTests = false;
        execution.AllowSellSubmissionTests = false;
        execution.AllowOfferWriteTests = false;
        execution.AllowProfileWriteTests = false;
    }

    public static bool IsProduction(TestSettings settings)
    {
        if (settings.Execution.Environment == EnvironmentType.Prod)
            return true;
        if (!Uri.TryCreate(settings.Execution.BaseUrl, UriKind.Absolute, out var uri))
            return false;
        return settings.Execution.ProductionHosts.Any(host => uri.Host.Equals(host, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Returns null when the write is allowed, otherwise the reason it is blocked.
    /// Tests call this and Assert.Ignore with the reason, so a blocked write is visible in the report.
    /// </summary>
    public static string? WhyBlocked(TestSettings settings, WriteKind kind)
    {
        if (IsProduction(settings))
            return $"{kind} tests never run against production ({settings.Execution.BaseUrl}).";

        var allowed = kind switch
        {
            WriteKind.Destructive => settings.Execution.AllowDestructiveTests,
            WriteKind.PaymentWrite => settings.Execution.AllowPaymentWriteTests,
            WriteKind.SellSubmission => settings.Execution.AllowSellSubmissionTests,
            WriteKind.OfferWrite => settings.Execution.AllowOfferWriteTests,
            WriteKind.ProfileWrite => settings.Execution.AllowProfileWriteTests,
            _ => false
        };

        return allowed
            ? null
            : $"{kind} tests are disabled. Set Execution.Allow{SwitchName(kind)}=true in config/appsettings.Dev.json (DEV only) to run them.";
    }

    private static string SwitchName(WriteKind kind) => kind switch
    {
        WriteKind.Destructive => "DestructiveTests",
        WriteKind.PaymentWrite => "PaymentWriteTests",
        WriteKind.SellSubmission => "SellSubmissionTests",
        WriteKind.OfferWrite => "OfferWriteTests",
        WriteKind.ProfileWrite => "ProfileWriteTests",
        _ => kind.ToString()
    };
}

public enum WriteKind
{
    Destructive,
    PaymentWrite,
    SellSubmission,
    OfferWrite,
    ProfileWrite
}
