namespace Carabia.Automation.Framework.Diagnostics;

/// <summary>Everything known about a finished test, independent of NUnit types.</summary>
public sealed class TestFailureContext
{
    public string TestName { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public IReadOnlyList<string> Categories { get; init; } = [];
    public string Outcome { get; init; } = string.Empty;
    public string? Message { get; init; }
    public string? StackTrace { get; init; }
    public TimeSpan Duration { get; init; }
}
