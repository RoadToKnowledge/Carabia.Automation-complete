using System.Text;
using Carabia.Automation.Framework.Base;
using Carabia.Automation.Framework.Reporting;
using Carabia.Automation.Framework.Utilities;

namespace Carabia.Automation.Framework.Diagnostics;

/// <summary>
/// On failure: screenshot, current URL, test name, categories, browser, environment, duration,
/// exception, stack trace and browser console. Every step is isolated, so a broken browser can
/// never hide the original test failure.
/// </summary>
public static class FailureArtifactCollector
{
    public sealed record Result(string Directory, string? ScreenshotPath, string ReportPath);

    public static Result Capture(BrowserSession? session, TestFailureContext context)
    {
        var settings = session?.Settings ?? Configuration.ConfigurationLoader.Load();
        var directory = ReportPaths.TestArtifactsDirectory(settings, context.TestName);
        Directory.CreateDirectory(directory);

        string? screenshot = null;
        string url = "(no browser)";
        IReadOnlyList<string> console = [];

        if (session is not null)
        {
            url = session.CurrentUrl;
            if (settings.Execution.ScreenshotOnFailure)
            {
                try
                {
                    screenshot = ScreenshotHelper.Capture(session.Driver, directory, "failure");
                }
                catch (Exception exception)
                {
                    screenshot = null;
                    session.Log($"(Screenshot could not be captured: {exception.Message})");
                }
            }

            try
            {
                console = BrowserLogCollector.Collect(session.Driver, OpenQA.Selenium.LogLevel.All);
                if (console.Count > 0)
                    File.WriteAllLines(Path.Combine(directory, "browser-console.log"), console);
            }
            catch (Exception)
            {
                // Logs are optional.
            }
        }

        var report = new StringBuilder()
            .AppendLine($"Test:        {context.FullName}")
            .AppendLine($"Outcome:     {context.Outcome}")
            .AppendLine($"Categories:  {string.Join(", ", context.Categories)}")
            .AppendLine($"Duration:    {context.Duration}")
            .AppendLine($"Browser:     {session?.BrowserName ?? settings.Execution.Browser}")
            .AppendLine($"Environment: {settings.Execution.Environment} ({settings.BaseUrl})")
            .AppendLine($"Role:        {session?.Role.ToString() ?? "-"}")
            .AppendLine($"URL:         {url}")
            .AppendLine($"Screenshot:  {screenshot ?? "(none)"}")
            .AppendLine()
            .AppendLine("Message:")
            .AppendLine(context.Message)
            .AppendLine()
            .AppendLine("Stack trace:")
            .AppendLine(context.StackTrace)
            .AppendLine()
            .AppendLine($"Browser console ({console.Count} entries) -> browser-console.log");

        var reportPath = Path.Combine(directory, "failure.txt");
        try
        {
            File.WriteAllText(reportPath, report.ToString());
        }
        catch (IOException)
        {
        }

        return new Result(directory, screenshot, reportPath);
    }
}
