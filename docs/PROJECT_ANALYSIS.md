# Project analysis

## Inputs

| Input | Role |
|---|---|
| `_Carabia_-ReleaseCarabia0079_` (Carabia.Web, ASP.NET Core 8 MVC) | Source of truth for routes, views, selectors, JS behaviour. Read-only. |
| `carabia-ui-tests-main` (xUnit + Selenium 4.48) | Working tests verified on dev.carabiacars.com. Its selectors and flows were ported. |
| `NUnitSeleniumAutomationFramework-master` | Reference for NUnit structure, POM layering, setup/teardown and reporting ideas only. |
| `Carabia.Automation` (partial, previous attempt) | Solution skeleton completed in this work (see `CONTINUATION_AUDIT.md`). |

## Application facts that shape the automation

- **Routing** (`Program.cs`): conventional routes plus custom ones - `brand/{brand}[/location-{location}][/page-N]`,
  `sellings/{id}_{modelName}`, `sellings/{status}` (sold, upcoming), `sellings/{filter1}` (collections from
  `General/SellingCollections`), `sellings/page-N` (N >= 2), `search` (POST), `info/{alias}`, `{category}` (news
  categories), `news/{alias}`, areas.
- **CanonicalMiddleware**: any GET path with upper-case letters -> 301 to lower case; trailing slash -> 301 without it.
  Canonical `<link>` tags are hard-coded to `https://carabiacars.com` + path.
- **Status pages**: `UseStatusCodePagesWithReExecute("/System/NotFoundPage")` renders `#notfound`.
- **Authentication**: cookie scheme `Carabia.Auth` (30 days, sliding). Login path is `/` (popup). Clients log in with
  phone + OTP (IdentityController.SendPhoneOtp / VerifyPhoneOtp), optional Google/Apple. Admins/managers log in at
  `/Identity/Authorization` with username/password. `GET /Identity/AuthStatus` returns
  `{ isAuthenticated, isClient, name }` - used to detect a completed manual login.
- **Roles**: `client` (Personal area), `admin`, `manager` (Administrator area; managers see fewer modules).
- **Sell My Car** (`/personal/sell`, GET AllowAnonymous): 11 screens in one view (`Views/Personal/Sell.cshtml`, ~7000 lines).
  VIN decode and manual Year/Make/Model/Spec/Regional-spec via `/api/autodata/sell-your-car/*`; Google Places location;
  PUG photo uploader (min 4 photos); listing qualification (client only); paid plans/boosts with Stripe card element.
- **Offers**: `/Payment/PaymentDetails?sellingId=N` (anonymous allowed). Minimum offer 75 % of asking price.
  Signed-in clients submit offers without a card (`ConfirmPayment` with `paymentIntentId = null`). Sellers handle offers
  in My Garage (`garage.js`: GetActiveOfferInfo, AcceptOffer, DeclineOffer, MakeOffer).
- **Payments**: Stripe. `appsettings.json` of the release contains LIVE Stripe keys, so no card test can run safely
  unless DEV is switched to test keys.
- **Auctions**: disabled in this release - `/auctions` shows "coming soon" unless `?auctions_visible` is present;
  `/auctions/sold|upcoming` redirect to `/auctions`.
- **Administrator area**: 20 controllers. Remove actions are plain GET links protected only by a JS `confirm()`.

## Architecture of the automation

```
Tests (NUnit)  ->  Flows  ->  Pages  ->  Components  ->  BrowserSession (driver, waits, step banner, log)
                                   \->  Utilities (WaitHelper, HttpProbe, UrlHelper, TextParser, TestDataReader)
Authentication: ManualAuthenticationManager -> SessionManager / CookieStorage / LocalStorageManager / AuthStatusProbe
Configuration:  ConfigurationLoader (JSON + env) -> TestSettings -> SafetyGuard
Diagnostics/Reporting: FailureArtifactCollector, BrowserLogCollector, ReportManager (HTML), NUnit XML via adapter
```

- One browser per fixture (fresh profile = clean state, no logout needed); per-test viewport reset, artifacts, report row.
- Authenticated fixtures inherit `BuyerTestBase`, `SellerTestBase` or `AdminTestBase` and are `[NonParallelizable]`.
- All waits are explicit; the only sleep is the optional presentation delay for visible runs (0 in headless).
- No raw selectors in test classes: they live in `src/.../Pages` and `src/.../Components`.
