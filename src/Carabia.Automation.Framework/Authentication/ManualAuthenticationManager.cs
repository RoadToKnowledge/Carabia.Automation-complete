using Carabia.Automation.Framework.Base;
using Carabia.Automation.Framework.Components.Common;
using Carabia.Automation.Framework.Utilities;

namespace Carabia.Automation.Framework.Authentication;

/// <summary>
/// MANUAL AUTHENTICATION + SAVED SESSION.
///
///   browser opens
///   -> saved session for the role? restore cookies/storage and verify via /Identity/AuthStatus
///   -> valid: continue automatically
///   -> missing/expired: open the right login screen, print
///        "MANUAL AUTHENTICATION REQUIRED FOR: SELLER"
///      and wait while the person logs in (phone + OTP, or admin username + password)
///   -> detect the authenticated state, save cookies + storage for that role, continue.
///
/// Selenium never types a phone number, password or OTP and never clicks Logout.
/// </summary>
public sealed class ManualAuthenticationManager(BrowserSession session)
{
    private readonly SessionManager _sessions = new(session.Settings);

    /// <returns>True if the person had to log in manually in this call, false if a saved session was reused.</returns>
    public bool EnsureAuthenticated(SessionRole role)
    {
        if (role == SessionRole.Public)
            return false;

        if (TryRestore(role))
            return false;

        if (IsNonInteractive())
            throw new ManualAuthenticationRequiredException(
                $"MANUAL AUTHENTICATION REQUIRED FOR: {role.Upper()} - but this run is non-interactive (CI). " +
                $"Create the session on a desktop first: scripts/create-session.ps1 -Role {role}.");

        PromptAndWait(role);
        var snapshot = _sessions.Save(role, session.Driver);
        session.Log($"SESSION SAVED FOR {role.Upper()}: {_sessions.GetSessionFilePath(role)} ({snapshot.Cookies.Count} cookies). " +
                    "This file is git-ignored; delete it to force a new login.");
        session.Step($"PASS: signed in as {role}. Automated checks continue now.");
        return true;
    }

    private bool TryRestore(SessionRole role)
    {
        if (!_sessions.TryLoad(role, out var snapshot, out var reason) || snapshot is null)
        {
            session.Log($"[{role}] {reason}.");
            return false;
        }

        _sessions.Restore(session.Driver, snapshot);
        var valid = SessionValidator.WaitUntilAuthenticated(session.Driver, role, TimeSpan.FromSeconds(10));
        if (valid && role == SessionRole.Administrator)
            valid = SessionValidator.AdministratorAreaOpens(session.Driver, session.BaseUrl,
                session.Settings.Authentication.AdministratorStartPath, session.DefaultTimeout);

        if (valid)
        {
            session.Log($"[{role}] saved session restored from {_sessions.GetSessionFilePath(role)} (captured {snapshot.CapturedAtUtc:u}).");
            return true;
        }

        session.Log($"[{role}] saved session has expired on the server. It will be replaced after you log in again.");
        _sessions.Delete(role);
        session.ResetToAnonymous();
        return false;
    }

    private void PromptAndWait(SessionRole role)
    {
        var timeout = TimeSpan.FromSeconds(session.Settings.Execution.ManualAuthenticationTimeoutSeconds);
        var banner = $"MANUAL AUTHENTICATION REQUIRED FOR: {role.Upper()}";
        var instructions = role == SessionRole.Administrator
            ? "Enter the administrator username and password in the browser."
            : $"Use the {role} test account: enter the phone number, request the OTP, type it and finish any sign-up step.";

        Console.WriteLine();
        Console.WriteLine(new string('=', banner.Length));
        Console.WriteLine(banner);
        Console.WriteLine(new string('=', banner.Length));
        Console.WriteLine($"{instructions} Waiting up to {timeout.TotalMinutes:0} minutes...");
        session.Log($"{banner}. {instructions}");

        if (role == SessionRole.Administrator)
        {
            session.Open(session.Settings.Authentication.AdministratorLoginPath);
        }
        else
        {
            session.Open(session.Settings.Authentication.ClientLoginPath);
            new LoginPopupComponent(session).OpenForManualLogin(session.Settings.Authentication.PreselectCountryCode);
        }

        session.Step($"{banner}. {instructions} You have {timeout.TotalMinutes:0} minutes.");

        var authenticated = WaitHelper.TryUntil(session.Driver, d => SessionValidator.IsAuthenticatedFor(d, role), timeout);
        if (!authenticated)
            throw new ManualAuthenticationRequiredException(
                $"{banner}: login was not completed within {timeout.TotalMinutes:0} minutes. " +
                (role == SessionRole.Administrator
                    ? "Make sure the account is an administrator/manager (client accounts cannot open /Administrator)."
                    : "Make sure the account is a client account and the OTP step was finished."));

        if (role == SessionRole.Administrator &&
            !SessionValidator.AdministratorAreaOpens(session.Driver, session.BaseUrl, session.Settings.Authentication.AdministratorStartPath, session.LongTimeout))
            throw new ManualAuthenticationRequiredException(
                "Logged in, but the Administrator area did not open. The account must have the 'admin' (or 'manager') role.");
    }

    private bool IsNonInteractive() =>
        string.Equals(Environment.GetEnvironmentVariable("CI"), "true", StringComparison.OrdinalIgnoreCase)
        || string.Equals(Environment.GetEnvironmentVariable("TF_BUILD"), "true", StringComparison.OrdinalIgnoreCase)
        || string.Equals(Environment.GetEnvironmentVariable("CARABIA_NON_INTERACTIVE"), "true", StringComparison.OrdinalIgnoreCase);
}
