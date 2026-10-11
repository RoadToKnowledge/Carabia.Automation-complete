# Carabia.Automation

Selenium + NUnit (.NET 8) UI automation for the Carabia used-car marketplace (Carabia.Web, release 0079).

- Page Object Model built from the real Carabia Razor views and JavaScript (selectors are listed in each page object).
- Logic proven by the working `carabia-ui-tests` project was ported into this framework (filters, footer, sitemap links,
  FAQ, sell wizard, profile, manual login detection).
- **Authentication is manual.** You log in once per role (phone + OTP, or admin password); the framework saves the
  browser session and reuses it. Selenium never types a phone number, password or OTP and never clicks Logout.
- Write and destructive tests are switched off by default and can never run against production.

## Prerequisites

| Need | Notes |
|---|---|
| .NET 8 SDK (or newer SDK) | `dotnet --list-sdks`. `global.json` accepts 8.0.100 and later (rolls forward). |
| Google Chrome or Microsoft Edge | Selenium Manager downloads the matching driver automatically. |
| Network access to the DEV site | `https://dev.carabiacars.com` by default. |
| Test accounts | One **Buyer** client account, one **Seller** client account (different phone numbers), one **admin** account. |

## Configure the environment

`config/appsettings.json` holds defaults; `config/appsettings.Dev.json` sets the DEV URL:

```json
{ "Execution": { "BaseUrl": "https://dev.carabiacars.com", "Environment": "Dev" } }
```

Choose another file with `$env:CARABIA_AUTOMATION_ENV = "Prod"` (read-only) or `"Local"` (`http://localhost:51031`).
Any value can be overridden by environment variables, e.g. `$env:CARABIA_AUTOMATION__Execution__Headless = "true"`.
Short aliases from carabia-ui-tests still work: `CARABIA_BASE_URL`, `CARABIA_HEADLESS`, `CARABIA_BROWSER`.

## Restore and build

```powershell
dotnet restore .\Carabia.Automation.sln
dotnet build .\Carabia.Automation.sln --configuration Debug
dotnet test .\Carabia.Automation.sln --no-build --list-tests     # test discovery (421 test cases)
```

Visual Studio: open `Carabia.Automation.sln`, build, then use Test Explorer (group by Traits to see categories).

## Manual authentication (once per role)

```powershell
.\scripts\create-session.ps1 -Role Buyer
.\scripts\create-session.ps1 -Role Seller
.\scripts\create-session.ps1 -Role Administrator
```

A visible browser opens and the console shows `MANUAL AUTHENTICATION REQUIRED FOR: SELLER`. Log in yourself
(the client login popup is opened for you with the country pre-selected; admins get `/Identity/Authorization`).
When `/Identity/AuthStatus` reports the right role, cookies and storage are saved to `sessions/<role>/session.json`
and the tests continue. Later runs reuse the session until it expires. Details: `docs/MANUAL_AUTHENTICATION.md`.
You do not have to create sessions first - any Buyer/Seller/Admin test asks for the login when needed.

## Run tests

| Suite | Command |
|---|---|
| Public (visitor, no login) | `.\scripts\run-public.ps1` (add `-Headless`, `-Workers 4`) |
| Smoke | `.\scripts\run-smoke.ps1` (public) / `.\scripts\run-smoke.ps1 -IncludeAuthenticated` |
| Buyer | `.\scripts\run-buyer.ps1` |
| Seller | `.\scripts\run-seller.ps1` |
| Admin | `.\scripts\run-admin.ps1` |
| Regression | `.\scripts\run-regression.ps1` (visitor) / `-IncludeAuthenticated` (all roles) |
| Chrome / Edge | `.\scripts\run-chrome.ps1`, `.\scripts\run-edge.ps1` |
| Write/destructive (DEV only) | `.\scripts\run-destructive.ps1` after enabling switches (below) |
| Clean outputs | `.\scripts\clean-results.ps1` (`-Sessions` also deletes saved logins) |

Plain `dotnet test` works too, e.g. `dotnet test --filter "TestCategory=SellFlow"` or
`dotnet test --filter "FullyQualifiedName~FilterTests"`. Categories: Smoke, Public, Buyer, Seller, Admin, Search,
Listings, SellFlow, Offers, Payments, Personal, Auctions, Articles, SEO, Responsive, Regression, Destructive,
WritesData, RequiresManualAuthentication, KnownIssue, SessionSetup.

## Results

| Output | Location |
|---|---|
| HTML report (test, status, category, browser, environment, duration, failure, screenshot) | `reports/html/<runId>/report.html` and `reports/html/latest.html` |
| NUnit XML | `reports/nunit/<runId>/` (scripts) |
| TRX | `reports/trx/<runId>.trx` (scripts) |
| Failure screenshot, URL, exception, stack trace, browser console | `artifacts/<runId>/<test name>/` |

## Safety

- Defaults: `AllowDestructiveTests`, `AllowPaymentWriteTests`, `AllowSellSubmissionTests`, `AllowOfferWriteTests`,
  `AllowProfileWriteTests` are all `false`. A blocked write test is reported as **Ignored** with the reason.
- Production (`Environment=Prod` or host `carabiacars.com`) forces every switch to `false` (`SafetyGuard`).
- Payment card tests additionally require the page's Stripe key to start with `pk_test_`; only Stripe test cards are used.
  The current Carabia configuration uses a live key, so they stay ignored (see `docs/KNOWN_LIMITATIONS.md`).
- Admin "Remove" links are GET requests; tests never follow them except the guarded FAQ test, which removes only the
  `AUTO-...` category it created itself.
- Offer write tests act only on the listing id you put in `testdata/Offers/OfferData.json` (an automation listing).
- `sessions/`, `artifacts/`, `reports/` are git-ignored. Never commit session files.

## Layout

```
src/Carabia.Automation.Framework   Configuration, Browser, Authentication, Base, Components, Pages, Flows, Models,
                                   Utilities, Diagnostics, Reporting
tests/Carabia.Automation.Tests     NUnit suites: Smoke, Authentication, Public, Search, Listings, SellFlow, Offers,
                                   Payments, Personal, Auctions, Admin, Seo, Responsive
config/   testdata/   scripts/   docs/   sessions/   .github/workflows/
```

Documentation: `docs/PROJECT_ANALYSIS.md`, `docs/CARABIA_MODULE_INVENTORY.md`, `docs/ROUTE_INVENTORY.md`,
`docs/TEST_COVERAGE_MATRIX.md`, `docs/MANUAL_AUTHENTICATION.md`, `docs/TEST_EXECUTION.md`, `docs/TEST_DATA_STRATEGY.md`,
`docs/SELECTOR_STRATEGY.md`, `docs/KNOWN_LIMITATIONS.md`, `docs/CI_CD_GUIDE.md`, `docs/IMPLEMENTATION_PROGRESS.md`,
`docs/CONTINUATION_AUDIT.md`.
