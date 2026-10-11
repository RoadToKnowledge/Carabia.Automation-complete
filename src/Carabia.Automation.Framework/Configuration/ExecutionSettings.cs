namespace Carabia.Automation.Framework.Configuration;

/// <summary>Values from the "Execution" section of config/appsettings*.json.</summary>
public sealed class ExecutionSettings
{
    public string BaseUrl { get; set; } = string.Empty;
    public EnvironmentType Environment { get; set; } = EnvironmentType.Dev;
    public string Browser { get; set; } = "Chrome";
    public bool Headless { get; set; }
    public int WindowWidth { get; set; } = 1920;
    public int WindowHeight { get; set; } = 1080;
    public int DefaultTimeoutSeconds { get; set; } = 20;
    public int PageLoadTimeoutSeconds { get; set; } = 60;
    public int ManualAuthenticationTimeoutSeconds { get; set; } = 600;
    public bool ScreenshotOnFailure { get; set; } = true;

    /// <summary>Shows the blue "TEST: ..." label inside the browser (same idea as carabia-ui-tests).</summary>
    public bool ShowStepBanner { get; set; } = true;

    /// <summary>Pause between visible walkthrough steps. Ignored (0) when headless.</summary>
    public int PresentationDelayMs { get; set; } = 400;

    public string ArtifactsDirectory { get; set; } = "artifacts";
    public string ReportsDirectory { get; set; } = "reports";
    public string SessionsDirectory { get; set; } = "sessions";
    public string TestDataDirectory { get; set; } = "testdata";

    // ---- Safety switches. All default to false and are forced to false on production. ----
    public bool AllowDestructiveTests { get; set; }
    public bool AllowPaymentWriteTests { get; set; }
    public bool AllowSellSubmissionTests { get; set; }
    public bool AllowOfferWriteTests { get; set; }
    public bool AllowProfileWriteTests { get; set; }

    /// <summary>Host names that are always treated as production, whatever Environment says.</summary>
    public List<string> ProductionHosts { get; set; } = ["carabiacars.com", "www.carabiacars.com"];
}
