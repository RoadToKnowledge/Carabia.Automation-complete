using Carabia.Automation.Framework.Configuration;
using Carabia.Automation.Framework.Reporting;

// One browser at a time by default (visible walkthroughs, shared test accounts). Raise with
// "-- NUnit.NumberOfTestWorkers=N" for public read-only runs; authenticated fixtures are [NonParallelizable].
[assembly: LevelOfParallelism(1)]

namespace Carabia.Automation.Tests;

/// <summary>Run-level setup: prints the effective configuration and safety switches; writes the final HTML report.</summary>
[SetUpFixture]
public sealed class AssemblyHooks
{
    [OneTimeSetUp]
    public void BeforeRun()
    {
        var settings = ConfigurationLoader.Load();
        var e = settings.Execution;
        TestContext.Progress.WriteLine("==================== Carabia.Automation ====================");
        TestContext.Progress.WriteLine($"Run id:        {ReportPaths.RunId}");
        TestContext.Progress.WriteLine($"Root:          {settings.RootDirectory}");
        TestContext.Progress.WriteLine($"Environment:   {settings.EnvironmentName} -> {e.Environment} ({settings.BaseUrl})");
        TestContext.Progress.WriteLine($"Browser:       {e.Browser} (headless: {e.Headless})");
        TestContext.Progress.WriteLine($"Production:    {SafetyGuard.IsProduction(settings)}");
        TestContext.Progress.WriteLine($"Write switches: destructive={e.AllowDestructiveTests}, payment={e.AllowPaymentWriteTests}, " +
                                       $"sell-submit={e.AllowSellSubmissionTests}, offer={e.AllowOfferWriteTests}, profile={e.AllowProfileWriteTests}");
        TestContext.Progress.WriteLine("Authentication is MANUAL. Login/logout/OTP are never automated.");
        TestContext.Progress.WriteLine("============================================================");
    }

    [OneTimeTearDown]
    public void AfterRun()
    {
        var settings = ConfigurationLoader.Load();
        var path = ReportManager.Write(settings);
        TestContext.Progress.WriteLine($"HTML report: {path}");
        TestContext.Progress.WriteLine($"Artifacts:   {ReportPaths.RunArtifactsDirectory(settings)}");
    }
}
