namespace Carabia.Automation.Framework.Configuration;

/// <summary>Root of config/appsettings.json (+ appsettings.{Environment}.json + environment variables).</summary>
public sealed class TestSettings
{
    public ExecutionSettings Execution { get; set; } = new();
    public AuthenticationSettings Authentication { get; set; } = new();

    /// <summary>Absolute path of the Carabia.Automation folder (where Carabia.Automation.sln lives).</summary>
    public string RootDirectory { get; set; } = string.Empty;

    /// <summary>Name of the environment file that was loaded, for example "Dev".</summary>
    public string EnvironmentName { get; set; } = "Dev";

    public string BaseUrl => Execution.BaseUrl.TrimEnd('/');

    public string ResolvePath(string relativeOrAbsolute) =>
        Path.IsPathRooted(relativeOrAbsolute) ? relativeOrAbsolute : Path.Combine(RootDirectory, relativeOrAbsolute);

    public bool IsConfigured =>
        Utilities.UrlHelper.TryHttpUri(Execution.BaseUrl, out var uri)
        && !uri.Host.Contains("replace-with", StringComparison.OrdinalIgnoreCase)
        && !uri.Host.EndsWith(".example", StringComparison.OrdinalIgnoreCase);
}
