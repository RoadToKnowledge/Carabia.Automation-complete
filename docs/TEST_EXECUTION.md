# Test execution

## Quick start (Windows, PowerShell, from the Carabia.Automation folder)

```powershell
dotnet restore .\Carabia.Automation.sln
dotnet build .\Carabia.Automation.sln
.\scripts\run-public.ps1                     # visitor tests, visible Chrome, one at a time
.\scripts\run-public.ps1 -Headless -Workers 4
.\scripts\run-seller.ps1                     # asks for SELLER login the first time
```

## Filters (dotnet test)

| Goal | Filter |
|---|---|
| Everything a visitor can run | `TestCategory=Public&TestCategory!=RequiresManualAuthentication` |
| Smoke (public only) | `TestCategory=Smoke&TestCategory!=RequiresManualAuthentication` |
| One role | `TestCategory=Buyer` / `Seller` / `Admin` |
| One area | `TestCategory=SellFlow`, `Search`, `Listings`, `Offers`, `Payments`, `Personal`, `Auctions`, `Articles`, `SEO`, `Responsive` |
| Write tests only | `TestCategory=WritesData` |
| Exclude known application bugs | `...&TestCategory!=KnownIssue` |
| One class | `FullyQualifiedName~FilterTests` |
| One test | `FullyQualifiedName~SellValidationTests.Vin_Help_Expands_And_Collapses` |

## Enabling write tests (DEV only)

Edit `config/appsettings.Dev.json`:

```json
{ "Execution": {
    "AllowSellSubmissionTests": true,   // submits a free listing with an AUTO-... marker in the owner notes
    "AllowOfferWriteTests": true,       // buyer offer, seller counter/decline/accept on AutomationSellingId
    "AllowPaymentWriteTests": false,    // Stripe card tests (also need a pk_test_ key on the site)
    "AllowProfileWriteTests": true,     // renames the Buyer test account and restores it
    "AllowDestructiveTests": true       // admin FAQ category create/rename/remove (AUTO row only)
} }
```

Then `.\scripts\run-destructive.ps1`. Blocked tests show as **Ignored** with the reason in the report.

Offer write tests need `testdata/Offers/OfferData.json` -> `"AutomationSellingId": <id>`: submit a listing with
`SellerSellFlowTests.Seller_Can_Submit_A_Free_Listing_And_See_It_In_Submissions`, approve it in
Administrator > Sellings > Moderation (manually), then put its id there.

## Visible walkthrough vs headless

- Visible (default on DEV): blue "TEST: ..." label in the page, `PresentationDelayMs` pause between steps.
- Headless: `-Headless` or `CARABIA_AUTOMATION__Execution__Headless=true`; banner pauses become 0.
- Authenticated fixtures without a saved session are always visible (you must log in).

## Browsers and drivers

`Execution.Browser` = `Chrome` (default) or `Edge`. Selenium Manager downloads chromedriver/msedgedriver.
Offline machine: set `CARABIA_DRIVER_PATH` to a folder containing the driver, and optionally
`CARABIA_BROWSER_BINARY` to a specific browser executable.

## Outputs per run (`<runId>` = yyyyMMdd-HHmmss)

- `reports/html/<runId>/report.html`, `reports/html/latest.html`
- `reports/nunit/<runId>/*.xml` (NUnit 3 format), `reports/trx/<runId>.trx`
- `artifacts/<runId>/<test>/failure.png`, `failure.txt` (URL, browser, environment, role, message, stack),
  `browser-console.log`
- Visual Studio Test Explorer also shows the failure screenshot as a test attachment.

## Parallelism

Default one worker (`[assembly: LevelOfParallelism(1)]`, `carabia.runsettings`). Public fixtures are
`[Parallelizable(ParallelScope.Self)]` so `-Workers N` runs N fixtures (N browsers) at once. Buyer/Seller/Admin
fixtures are `[NonParallelizable]` because they share one account per role.

## Expected data on DEV

Active listings including Audi and BMW, at least one listing with 2+ photos, published info pages
(how-it-works, about-us, faq, privacy-policy, user-agreement), news categories news/overviews/buying-guides/
selling-guides/partners, Autodata and Google Maps keys working (sell wizard). Tests that depend on optional data
report **Inconclusive** instead of passing silently.
