using System.Net;
using System.Text;
using Carabia.Automation.Framework.Configuration;

namespace Carabia.Automation.Framework.Reporting;

/// <summary>
/// Lightweight HTML execution report (no third-party reporting package).
/// Rewritten after every test so a cancelled run still leaves a readable report.
/// NUnit's own XML result is produced by the NUnit adapter (see scripts/ and docs/TEST_EXECUTION.md).
/// </summary>
public static class ReportManager
{
    private static readonly object SyncRoot = new();
    private static readonly List<ReportEntry> Entries = [];
    private static readonly DateTime StartedAt = DateTime.Now;

    public static IReadOnlyList<ReportEntry> Snapshot()
    {
        lock (SyncRoot) return Entries.ToList();
    }

    public static void Record(TestSettings settings, ReportEntry entry)
    {
        lock (SyncRoot)
        {
            Entries.Add(entry);
            Write(settings);
        }
    }

    public static string Write(TestSettings settings)
    {
        lock (SyncRoot)
        {
            var path = ReportPaths.HtmlReportPath(settings);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            var html = Render(settings);
            File.WriteAllText(path, html, Encoding.UTF8);
            try
            {
                File.WriteAllText(ReportPaths.LatestHtmlReportPath(settings), html, Encoding.UTF8);
            }
            catch (IOException)
            {
            }
            return path;
        }
    }

    private static string Render(TestSettings settings)
    {
        var passed = Entries.Count(e => e.Status == "Passed");
        var failed = Entries.Count(e => e.Status == "Failed");
        var skipped = Entries.Count - passed - failed;
        var rows = new StringBuilder();
        foreach (var e in Entries)
        {
            var css = e.Status switch { "Passed" => "pass", "Failed" => "fail", _ => "skip" };
            var screenshot = string.IsNullOrEmpty(e.ScreenshotPath)
                ? ""
                : $"<a href=\"{Encode(new Uri(e.ScreenshotPath).AbsoluteUri)}\">screenshot</a>";
            rows.AppendLine($"<tr class=\"{css}\"><td>{Encode(e.TestName)}</td><td>{Encode(e.Status)}</td><td>{Encode(e.Category)}</td>" +
                            $"<td>{Encode(e.Browser)}</td><td>{Encode(e.Environment)}</td><td>{e.Duration.TotalSeconds:0.0}s</td>" +
                            $"<td><pre>{Encode(e.FailureMessage)}</pre></td><td>{screenshot}<br><small>{Encode(e.ScreenshotPath)}</small></td></tr>");
        }

        return $$"""
            <!doctype html>
            <html lang="en"><head><meta charset="utf-8"><title>Carabia Automation Report {{ReportPaths.RunId}}</title>
            <style>
              body { font-family: Segoe UI, Arial, sans-serif; margin: 24px; color: #111827; }
              table { width: 100%; border-collapse: collapse; font-size: 13px; }
              th, td { border: 1px solid #d1d5db; padding: 6px 8px; text-align: left; vertical-align: top; }
              th { background: #f3f4f6; position: sticky; top: 0; }
              tr.pass td:nth-child(2) { color: #047857; font-weight: 600; }
              tr.fail td:nth-child(2) { color: #b91c1c; font-weight: 600; }
              tr.skip td:nth-child(2) { color: #92400e; }
              pre { white-space: pre-wrap; margin: 0; max-width: 520px; }
              .summary span { margin-right: 18px; }
            </style></head>
            <body>
              <h1>Carabia Automation Report</h1>
              <p class="summary"><span>Run: {{ReportPaths.RunId}}</span><span>Started: {{StartedAt:yyyy-MM-dd HH:mm:ss}}</span>
                <span>Environment: {{Encode(settings.Execution.Environment.ToString())}} ({{Encode(settings.BaseUrl)}})</span></p>
              <p class="summary"><span>Total: {{Entries.Count}}</span><span>Passed: {{passed}}</span><span>Failed: {{failed}}</span><span>Skipped/Ignored: {{skipped}}</span></p>
              <table><thead><tr><th>Test</th><th>Status</th><th>Category</th><th>Browser</th><th>Environment</th><th>Duration</th><th>Failure message</th><th>Screenshot</th></tr></thead>
              <tbody>
            {{rows}}
              </tbody></table>
            </body></html>
            """;
    }

    private static string Encode(string? value) => WebUtility.HtmlEncode(value ?? string.Empty);
}

public sealed class ReportEntry
{
    public string TestName { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string Browser { get; init; } = string.Empty;
    public string Environment { get; init; } = string.Empty;
    public TimeSpan Duration { get; init; }
    public string FailureMessage { get; init; } = string.Empty;
    public string ScreenshotPath { get; init; } = string.Empty;
}
