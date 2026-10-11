# Test data strategy

## Where data lives

| File | Model | Used by |
|---|---|---|
| `testdata/Vehicles/SellVehicles.json` | `SellVehiclesFile` / `VehicleTestData` / `LocationTestData` | Sell wizard (VIN 2FMPK4G83GBB65542 = 2016 Ford Edge SE, GCC - verified by carabia-ui-tests), invalid VINs |
| `testdata/Search/SearchCases.json` | `SearchTestData` | makes with listings, collections, sort cases, FAQ landing pages, text queries |
| `testdata/Offers/OfferData.json` | `OfferTestData` | offer ratios (valid 90 %, too low 50 %, counter 95 %), `AutomationSellingId` |
| `testdata/Payments/StripeTestData.json` | `PaymentTestData` | Stripe public TEST cards only (4242..., 4000 0000 0000 0002) |
| `testdata/Admin/AdminData.json` | `AdminTestData` | AUTO FAQ category/question templates |
| `testdata/Public/PublicPages.json` | `PublicPagesData` (tests) | public routes, info aliases, article categories, personal routes, footer links, sitemap groups |
| `testdata/Uploads/photos/*.jpg` | - | 5 synthetic photos (generated; no real car or person) |
| `testdata/Uploads/invalid-file.txt` | - | invalid upload |

Test classes read data through `Tests/Base/TestData.cs` (typed, loaded once) and `[TestCaseSource]`. No test method
contains hard-coded URLs, selectors or credentials.

## Rules

- No passwords, OTPs, phone numbers, real customer data or production payment credentials in the repository.
- Everything automation creates carries a marker `AUTO-yyyyMMddHHmmss-XXXX` (`RandomDataGenerator.AutomationMarker()`):
  owner notes of submitted listings, admin FAQ category names.
- Destructive steps check `RandomDataGenerator.IsAutomationMarker(...)` before removing anything and only remove the
  row whose exact text they created in the same test.
- Write tests clean up after themselves where the application allows it (profile names restored, favourites
  un-toggled, FAQ category removed in `finally`). Submitted listings and offers cannot be deleted by a client;
  they stay as automation records (identifiable by the AUTO marker) - use one automation listing id for offers.
- Data-dependent expectations (featured listing 1056, Audi/BMW stock, prefix collections) are in JSON so they can be
  updated without code changes. Missing optional data gives **Inconclusive**, not a silent pass.
