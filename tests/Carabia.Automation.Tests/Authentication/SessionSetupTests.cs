using Carabia.Automation.Framework.Authentication;
using Carabia.Automation.Framework.Utilities;
using Carabia.Automation.Tests.Base;

namespace Carabia.Automation.Tests.Authentication;

/// <summary>
/// Run these to create (or refresh) a role session before a long suite:
///   scripts/create-session.ps1 -Role Seller
/// The fixture opens the browser, reuses sessions/seller/session.json if it is still valid, otherwise prints
/// "MANUAL AUTHENTICATION REQUIRED FOR: SELLER" and waits for you to log in. Nothing is typed for you.
/// </summary>
[TestFixture]
[Category("SessionSetup")]
public sealed class BuyerSessionSetupTests : BuyerTestBase
{
    [Test]
    public void Buyer_Session_Is_Saved_And_Valid() => SessionAssertions.AssertClientSessionSaved(Session, Settings, SessionRole.Buyer);
}

[TestFixture]
[Category("SessionSetup")]
public sealed class SellerSessionSetupTests : SellerTestBase
{
    [Test]
    public void Seller_Session_Is_Saved_And_Valid() => SessionAssertions.AssertClientSessionSaved(Session, Settings, SessionRole.Seller);
}

[TestFixture]
[Category("SessionSetup")]
public sealed class AdministratorSessionSetupTests : AdminTestBase
{
    [Test]
    public void Administrator_Session_Is_Saved_And_Opens_Administrator_Area()
    {
        var manager = new SessionManager(Settings);
        Assert.That(manager.TryLoad(SessionRole.Administrator, out _, out var reason), Is.True, $"Saved administrator session: {reason}");
        var status = Session.AuthStatus();
        Assert.That(status.IsAuthenticated && !status.IsClient, Is.True, $"Expected a non-client authenticated user; AuthStatus: {status}");
        Assert.That(SessionValidator.AdministratorAreaOpens(Session.Driver, Settings.BaseUrl, Settings.Authentication.AdministratorStartPath, Session.LongTimeout),
            Is.True, "The Administrator area did not open with this session.");
    }
}

internal static class SessionAssertions
{
    public static void AssertClientSessionSaved(Framework.Base.BrowserSession session, Framework.Configuration.TestSettings settings, SessionRole role)
    {
        var manager = new SessionManager(settings);
        Assert.That(manager.TryLoad(role, out var snapshot, out var reason), Is.True, $"Saved {role} session: {reason}");
        var status = session.AuthStatus();
        Assert.Multiple(() =>
        {
            Assert.That(status.IsAuthenticated && status.IsClient, Is.True, $"AuthStatus: {status}");
            Assert.That(snapshot!.Cookies.Any(c => c.Name == CookieStorage.AuthCookieName), Is.True, "The saved session has no Carabia.Auth cookie.");
            Assert.That(File.ReadAllText(manager.GetSessionFilePath(role)), Does.Not.Contain("password").IgnoreCase,
                "Session files must only contain cookies/storage.");
        });
        TestContext.Out.WriteLine($"{role} session file: {manager.GetSessionFilePath(role)} (captured {snapshot!.CapturedAtUtc:u}, {UrlHelper.PathOf(settings.BaseUrl)})");
    }
}
