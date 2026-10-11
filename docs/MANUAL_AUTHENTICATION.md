# Manual authentication and saved sessions

## Why login is manual

Carabia clients log in with a phone number and a one-time password sent by SMS (plus optional Google/Apple and
e-mail verification). Administrators log in with a username and password. Automating any of that would mean storing
phone numbers, passwords or OTP bypasses in a test repository. So login, logout, OTP, sign-up, recovery, social
login and CAPTCHA stay **manual**. Selenium automates everything after you are signed in.

## Roles

| Role | Account | Where you log in | Detected when `/Identity/AuthStatus` returns |
|---|---|---|---|
| Buyer | client test account #1 | home page "Sign in" popup (opened for you) | `isAuthenticated: true, isClient: true` |
| Seller | client test account #2 (different phone) | same popup | `isAuthenticated: true, isClient: true` |
| Administrator | admin (or manager) account | `/Identity/Authorization` | `isAuthenticated: true, isClient: false` **and** `/administrator/sellings` opens |

Buyer and Seller must be different accounts: Carabia hides "Contact the Owner" on your own listing, so offers
between them need two people. Managers see fewer admin modules than admins; admin-only modules will fail for a manager.

## What happens in a run

1. The fixture starts a browser. If no valid saved session exists for its role, the browser is forced visible.
2. `ManualAuthenticationManager` looks for `sessions/<role>/session.json`:
   - reused only if it belongs to the same `BaseUrl`, is younger than `Authentication.SessionMaxAgeHours` (480 h),
     and still has an unexpired `Carabia.Auth` cookie;
   - cookies and local/session storage are restored, the page is reloaded and `/Identity/AuthStatus` is asked.
3. Valid: tests continue immediately.
4. Missing/expired: the console prints

   ```
   =====================================
   MANUAL AUTHENTICATION REQUIRED FOR: SELLER
   =====================================
   ```

   and the browser shows the same text. For clients the "Sign in" popup is opened and the country code from
   `Authentication.PreselectCountryCode` (default `+91`) is pre-selected. For admins `/Identity/Authorization` opens.
5. You complete the login (phone, OTP, any sign-up step / username and password). The framework polls
   `/Identity/AuthStatus` every 250 ms for up to `ManualAuthenticationTimeoutSeconds` (600 s).
6. When the role is detected, cookies + storage are written to `sessions/<role>/session.json` and the tests continue.

## Create or refresh sessions up front

```powershell
.\scripts\create-session.ps1 -Role Buyer
.\scripts\create-session.ps1 -Role Seller
.\scripts\create-session.ps1 -Role Administrator
.\scripts\create-session.ps1 -Role Seller -Force     # delete the old one first
```

## Expiry

- Server side: `Carabia.Auth` lasts 30 days with sliding expiration (Program.cs). If the server no longer accepts it,
  the restore check fails, the file is deleted, cookies are cleared and you are asked to log in again.
- Client side: files older than `SessionMaxAgeHours` or with an expired auth cookie are ignored.
- CI / non-interactive runs (`CI=true`, `TF_BUILD=true` or `CARABIA_NON_INTERACTIVE=true`) never wait: they fail at
  once with "MANUAL AUTHENTICATION REQUIRED FOR: <ROLE> - but this run is non-interactive".

## Clean state without logout

Every fixture gets a new browser profile, so a fixture is either anonymous (Public) or exactly the role it asked for.
`BrowserSession.ResetToAnonymous()` deletes the site's cookies and storage when a test needs to drop a session.
`HeaderComponent.ClickPersonalMenuItem` and `PersonalPageBase.ClickNavigation` refuse "Log out".

## Git

`sessions/*` is in `.gitignore` (only `README.md` and `.gitkeep` are tracked). A session file is equivalent to being
logged in as that account - do not share or commit it.
