using Carabia.Automation.Framework.Configuration;
using Carabia.Automation.Framework.Utilities;

namespace Carabia.Automation.Framework.Reporting;

/// <summary>Artifacts and reports are separated per run: artifacts/&lt;runId&gt;/..., reports/html/&lt;runId&gt;/...</summary>
public static class ReportPaths
{
    public static string RunId { get; } =
        Environment.GetEnvironmentVariable("CARABIA_RUN_ID") is { Length: > 0 } id ? id : DateTime.Now.ToString("yyyyMMdd-HHmmss");

    public static string RunArtifactsDirectory(TestSettings settings) =>
        Path.Combine(settings.ResolvePath(settings.Execution.ArtifactsDirectory), RunId);

    public static string TestArtifactsDirectory(TestSettings settings, string testName) =>
        Path.Combine(RunArtifactsDirectory(settings), ScreenshotHelper.SafeFileName(testName));

    public static string HtmlReportDirectory(TestSettings settings) =>
        Path.Combine(settings.ResolvePath(settings.Execution.ReportsDirectory), "html", RunId);

    public static string HtmlReportPath(TestSettings settings) => Path.Combine(HtmlReportDirectory(settings), "report.html");

    public static string LatestHtmlReportPath(TestSettings settings) =>
        Path.Combine(settings.ResolvePath(settings.Execution.ReportsDirectory), "html", "latest.html");

    public static string NUnitResultsDirectory(TestSettings settings) =>
        Path.Combine(settings.ResolvePath(settings.Execution.ReportsDirectory), "nunit");
}
