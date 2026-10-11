# Known limitations and application findings

## Not executed against DEV yet

The build machine used to finish this framework had no network route to dev.carabiacars.com (and no NuGet access;
packages were taken from the official GitHub releases). Therefore:

- Restore/build/discovery were verified (0 errors, 0 warnings, 421 test cases, all runnable).
- The framework core was executed for real with Chromium 141 against a local stub server: configuration and
  environment overrides, production fail-safe, browser start, step banner, canonical/404 detection, AuthStatus,
  saved-session save -> restore in a fresh browser, non-interactive fail-fast, failure screenshot + failure.txt +
  console log, HTML report, HTTP redirect probe, test-data loading (6/6 passed).
- The Carabia test suites themselves have **not** been run against DEV. Selectors come from the release-0079 source
  and from carabia-ui-tests (verified on DEV). Expect a first-run tuning pass; record it in `IMPLEMENTATION_PROGRESS.md`.

## Application findings from the source (documented, not fixed - Carabia source is read-only)

| # | Finding | Where | Test that shows it |
|---|---|---|---|
| 1 | Photo uploader accepts any file type: `pugHandleUpload()` reads every file; only the OS dialog `accept` filters. A `.txt` becomes a broken "photo". | Views/Personal/Sell.cshtml | `SellerSellFlowTests.Non_Image_File_Is_Not_Accepted_As_A_Photo` (KnownIssue) |
| 2 | Personal Payments page title is "Notifications" (`ViewBag.PageTitle` copy/paste). | Views/Personal/Payments.cshtml | `BuyerPersonalAreaTests.Payments_Page_Title_Is_Payments` (KnownIssue) |
| 3 | The release `appsettings.json` contains **live** Stripe publishable and secret keys and a webhook secret. Card tests stay blocked; the secret key should be rotated and moved to a secret store. | Carabia.Web/appsettings.json | `MakeOfferPageTests.Offer_Page_Reports_Stripe_Mode_*`, `SellListingPaymentTests` (ignored) |
| 4 | HTML sitemap links to `/sellings/certified`, but that route is commented out in Program.cs (certified page removed) -> likely 404. | Views/System/Sitemap.cshtml | `BrokenLinkTests.Internal_Links_On_Page_Respond(/sitemap)` |
| 5 | Admin "Remove" actions are HTTP GET (`/administrator/{module}/remove/{id}`), guarded only by a JS confirm; ServiceRequests' Remove buttons have no confirm at all. A crawler/prefetch with an admin cookie could delete data. | Areas/Administrator | `AdminModuleTests.Remove_Links_Are_Protected_By_Confirmation` (warning for ServiceRequests) |
| 6 | carabia-ui-tests observed `/System/GetCarModels` HTTP 500 on DEV (model dropdown stays disabled) and Clear-all leaving max price blank (2026-09-15). | filters | `FilterTests.Selecting_A_Model_*`, `Clear_All_*` |
| 7 | Opening /personal/notifications marks all notifications as read (GET with side effect). Expected product behaviour, noted because tests open it. | PersonalController.Notifications | `BuyerPersonalAreaTests.Notifications_Page_Shows_Log` |
| 8 | Canonical tags are hard-coded to `https://carabiacars.com`, so on DEV they point to production. Tests compare paths only. | several views | `CanonicalAndRedirectTests` |
| 9 | Search (`/search`) is POST-only and no search box exists in the current header. | HomeController.Search | `TextSearchTests` (HTTP) |
| 10 | Auctions are switched off (`/auctions` coming-soon, status pages redirect). Bidding is not automated. | AuctionsController | `AuctionTests` |

## Intentionally not automated

- Login, logout, OTP, sign-up, recovery, Google/Apple, CAPTCHA, e-mail verification codes (manual by design).
- Sending test-drive requests, inspection/finance/shipping requests, comments, newsletter subscriptions,
  removal requests: they notify real people or create work items. The dialogs are opened and checked.
- Admin saves on real records, Autodata imports, moderation decisions on real submissions, creating admins.
- Card binding (`BindCard`, `SetActiveCard`), Stripe webhook, `RunSellingsJob`.

## Data dependencies

Filter/sort/collection tests need active listings (Audi, BMW, SUVs, cars under AED 100,000). The featured-listing
case (1056_audi-a6) and some optional checks report **Inconclusive** if the data is missing.
