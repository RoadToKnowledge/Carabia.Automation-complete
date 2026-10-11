# Implementation progress

## Status by area

| Area | Status | Notes |
|---|---|---|
| Solution, projects, references (.NET 8) | DONE | Both projects added to `Carabia.Automation.sln` (they were missing). Test -> Framework reference. |
| Configuration (JSON + env, Dev/Prod/Local) | DONE | DEV URL https://dev.carabiacars.com; array/env overrides; production fail-safe. |
| Browser (Chrome/Edge, Selenium Manager, headless, viewports) | DONE | Selenium 4.48.0 (same as carabia-ui-tests). |
| Manual authentication + saved sessions (Buyer/Seller/Admin) | DONE | AuthStatus-based detection; verified with real browser on a local stub. |
| Diagnostics + HTML/NUnit/TRX reporting | DONE | verified on local stub. |
| Common components (header, mobile drawer, footer, login popup, sitemap links, FAQ, pagination) | DONE | |
| Public: home, navigation, footer, info, articles, 404, FAQ | DONE | |
| Search: filters, sorting, pagination, collections, text search | DONE | |
| Listings: cards, details, gallery, features, map, contact owner, services | DONE / PARTIAL | request submissions not sent |
| Sell: VIN + manual, validation, location, notes, price, photos, summary | DONE | submission GUARDED |
| Personal: garage, settings, notifications, payments, favourites, listing management | DONE / GUARDED | |
| Offers: offer page rules; buyer offer; seller counter/decline/accept | DONE / GUARDED | needs AutomationSellingId |
| Payments: safety, payment step UI; Stripe card tests | DONE / BLOCKED | live Stripe key in release |
| Auctions | DONE | bidding BLOCKED (feature off) |
| Admin: all 20 controllers in catalog; list/table/tabs/forms/navigation/filters/moderation; FAQ CRUD | DONE / PARTIAL / GUARDED | |
| SEO: sitemap XML/HTML, canonical, redirects, broken links, titles | DONE | |
| Responsive: 3 viewports | DONE | |
| Smoke / Regression | DONE | categories across suites (no duplicated classes) |
| Scripts, CI, docs | DONE | |
| First run against DEV | **PENDING** | run `scripts/run-public.ps1`, then role suites; fix any selector drift here |

## Verification log

| Date | Check | Result |
|---|---|---|
| 2026-10-11 | `dotnet build Carabia.Automation.sln -c Debug` (local package feed) | 0 errors, 0 warnings |
| 2026-10-11 | NUnit discovery (nunit3-console --explore) | 421 test cases, 0 not runnable |
| 2026-10-11 | Category filters (Public/Buyer/Seller/Admin/WritesData/Destructive) | 265 / 35 / 24 / 99 / 15 / 1 |
| 2026-10-11 | Framework self-check with Chromium 141 + chromedriver 141 against local stub | 6/6 passed |
| 2026-10-11 | Lifecycle check (fixture browser, per-test artifacts, HTML report rows with categories) | working |
| - | Public suite on DEV | not run (no network to DEV from build machine) |
| - | Buyer/Seller/Admin suites on DEV | not run (needs manual login) |

## Next steps

1. On your PC: `dotnet restore`, `dotnet build`, `dotnet test --list-tests`.
2. `scripts/run-public.ps1` (visible) - fix any data/selector differences on DEV.
3. Create sessions (`scripts/create-session.ps1 -Role Buyer|Seller|Administrator`) and run role suites.
4. Optional: enable write switches on DEV, create the automation listing, set `AutomationSellingId`.
