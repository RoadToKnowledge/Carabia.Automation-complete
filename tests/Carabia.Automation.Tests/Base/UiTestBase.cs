using System.Diagnostics;
using Carabia.Automation.Framework.Authentication;
using Carabia.Automation.Framework.Base;
using Carabia.Automation.Framework.Browser;
using Carabia.Automation.Framework.Configuration;
using Carabia.Automation.Framework.Diagnostics;
using Carabia.Automation.Framework.Reporting;
using Carabia.Automation.Framework.Utilities;
using NUnit.Framework.Interfaces;

namespace Carabia.Automation.Tests.Base;

/// <summary>
/// One browser per test fixture (like carabia-ui-tests' IClassFixture&lt;BrowserFixture&gt;), fresh for every fixture,
/// so no state leaks between fixtures and no logout is ever needed. Per test: viewport reset, timing,
/// failure artifacts (screenshot, URL, console, exception) and an entry in the HTML report.
/// </summary>
public abstract class UiTestBase
{
    private Stopwatch _stopwatch = new();
    private BrowserSession? _session;

    protected TestSettings Settings { get; private set; } = null!;

    /// <summary>The role this fixture runs as. Public fixtures never authenticate.</summary>
    protected virtual SessionRole Role => SessionRole.Public;

    protected BrowserSession Session => _session ?? throw new InvalidOperationException("The browser session was not started.");

    [OneTimeSetUp]
    public void StartBrowser()
    {
        Settings = ConfigurationLoader.Load();
        if (!Settings.IsConfigured)
            Assert.Ignore("Execution.BaseUrl is not configured. Set it in config/appsettings.Dev.json (or CARABIA_BASE_URL).");

        var forceVisible = Role != SessionRole.Public && NeedsManualLogin();
        _session = BrowserSession.Start(Settings, Role, Log, forceVisible);
        try
        {
            if (Role != SessionRole.Public)
                new ManualAuthenticationManager(_session).EnsureAuthenticated(Role);
            OnBrowserReady();
        }
        catch
        {
            CaptureFixtureFailure();
            _session.Dispose();
            _session = null;
            throw;
        }
    }

    /// <summary>Hook for fixture-level preparation after the (authenticated) browser is ready.</summary>
    protected virtual void OnBrowserReady()
    {
    }

    [SetUp]
    public void BeforeEachTest()
    {
        _stopwatch = Stopwatch.StartNew();
        if (_session is null) return;
        Log($"--- {TestContext.CurrentContext.Test.Name}");
        try
        {
            WindowManager.Resize(Session.Driver, new Viewport("Default", Settings.Execution.WindowWidth, Settings.Execution.WindowHeight));
        }
        catch (OpenQA.Selenium.WebDriverException)
        {
            // Headless windows accept the size from the --window-size argument; resizing is best effort.
        }
    }

    [TearDown]
    public void AfterEachTest()
    {
        _stopwatch.Stop();
        var context = TestContext.CurrentContext;
        var status = context.Result.Outcome.Status;
        string screenshot = string.Empty;

        if (status == TestStatus.Failed)
        {
            try
            {
                var artifacts = FailureArtifactCollector.Capture(_session, new TestFailureContext
                {
                    TestName = context.Test.Name,
                    FullName = context.Test.FullName,
                    Categories = Categories(),
                    Outcome = context.Result.Outcome.ToString(),
                    Message = context.Result.Message,
                    StackTrace = context.Result.StackTrace,
                    Duration = _stopwatch.Elapsed
                });
                screenshot = artifacts.ScreenshotPath ?? string.Empty;
                TestContext.Out.WriteLine($"Failure artifacts: {artifacts.Directory}");
                if (artifacts.ScreenshotPath is not null)
                    TestContext.AddTestAttachment(artifacts.ScreenshotPath, "Failure screenshot");
            }
            catch (Exception exception)
            {
                // Never let artifact collection replace the real failure.
                TestContext.Out.WriteLine($"(Failure artifacts could not be collected: {exception.Message})");
            }
        }

        try
        {
            ReportManager.Record(Settings ?? ConfigurationLoader.Load(), new ReportEntry
            {
                TestName = context.Test.FullName,
                Status = status.ToString(),
                Category = string.Join(", ", Categories()),
                Browser = _session?.BrowserName ?? Settings?.Execution.Browser ?? string.Empty,
                Environment = Settings is null ? string.Empty : $"{Settings.Execution.Environment} ({Settings.BaseUrl})",
                Duration = _stopwatch.Elapsed,
                FailureMessage = status is TestStatus.Passed ? string.Empty : context.Result.Message ?? string.Empty,
                ScreenshotPath = screenshot
            });
        }
        catch (IOException exception)
        {
            TestContext.Out.WriteLine($"(Report could not be written: {exception.Message})");
        }
    }

    [OneTimeTearDown]
    public void StopBrowser()
    {
        _session?.Dispose();
        _session = null;
    }

    // ---------- helpers for tests ----------

    protected void Log(string message) => TestContext.Progress.WriteLine($"[{DateTime.Now:HH:mm:ss}] {message}");

    protected void Step(string message) => Session.Step(message);

    /// <summary>Skips (Ignore) a write test with the exact reason when SafetyGuard blocks it.</summary>
    protected void RequireWritePermission(WriteKind kind)
    {
        var reason = SafetyGuard.WhyBlocked(Settings, kind);
        if (reason is not null)
            Assert.Ignore(reason);
    }

    protected HttpProbe NewHttpProbe() => new(Settings.BaseUrl, Settings.Execution.PageLoadTimeoutSeconds);

    /// <summary>Method categories plus the fixture's (and its base classes') categories.</summary>
    protected IReadOnlyList<string> Categories()
    {
        var categories = new List<string>();
        if (TestContext.CurrentContext.Test.Properties["Category"] is { } own)
            categories.AddRange(own.Cast<object>().Select(o => o.ToString()!));
        for (var type = GetType(); type is not null && type != typeof(object); type = type.BaseType)
            categories.AddRange(type.GetCustomAttributes(typeof(CategoryAttribute), false).Cast<CategoryAttribute>().Select(c => c.Name));
        return categories.Distinct().ToList();
    }

    private bool NeedsManualLogin() =>
        !new SessionManager(Settings).TryLoad(Role, out _, out _);

    private void CaptureFixtureFailure()
    {
        try
        {
            FailureArtifactCollector.Capture(_session, new TestFailureContext
            {
                TestName = $"{GetType().Name}-setup",
                FullName = GetType().FullName ?? GetType().Name,
                Outcome = "Fixture setup failed"
            });
        }
        catch (Exception)
        {
            // best effort
        }
    }
}

/// <summary>Fixtures signed in as the Buyer test account (manual login once, then the saved session).</summary>
[Category(TestCategories.Buyer)]
[Category(TestCategories.RequiresManualAuthentication)]
[NonParallelizable]
public abstract class BuyerTestBase : UiTestBase
{
    protected override SessionRole Role => SessionRole.Buyer;
}

/// <summary>Fixtures signed in as the Seller test account.</summary>
[Category(TestCategories.Seller)]
[Category(TestCategories.RequiresManualAuthentication)]
[NonParallelizable]
public abstract class SellerTestBase : UiTestBase
{
    protected override SessionRole Role => SessionRole.Seller;
}

/// <summary>Fixtures signed in as an administrator (username/password at /Identity/Authorization, typed by a person).</summary>
[Category(TestCategories.Admin)]
[Category(TestCategories.RequiresManualAuthentication)]
[NonParallelizable]
public abstract class AdminTestBase : UiTestBase
{
    protected override SessionRole Role => SessionRole.Administrator;
}
