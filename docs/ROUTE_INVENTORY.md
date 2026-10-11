# Route inventory (from Carabia.Web source)

All paths are lower-cased by CanonicalMiddleware (GET with capitals -> 301). Trailing slashes -> 301.

## Conventional / custom routes (`Program.cs`)

| Name | Pattern | Target | Notes / tests |
|---|---|---|---|
| brand_location | `brand/{brand}/location-{location}/{page?}` | Home.Brand | unknown brand/location -> 404 |
| brands | `brand/{brand}/{page?}` | Home.Brand | `HomeTests`, `FaqTests`, `ErrorPageTests` |
| auctions | `auctions/{id:int}_{modelName}` | Auctions.Auction | `AuctionTests` |
| sellings | `sellings/{id:int}_{modelName}` | Sellings.Selling | wrong modelName -> 404 (`ListingDetailsTests`) |
| auctions_status | `auctions/{status}/{page?}` (sold, upcoming) | Auctions.Index | redirects to `/auctions` |
| auctions_page | `auctions/page-N` | Auctions.Index | |
| sellings_status | `sellings/{status}/{page?}` (sold, upcoming) | Sellings.Index | `/sellings/sold` = "Previously Listed" |
| sellings_filter_1 | `sellings/{filter1}/{page?}` | Sellings.Index | quick filters evs, gcc, chinese-brands, family-car, ultra-low-mileage, low-budget, under-warranty, suvs; prefixes body-, location-, model-, under- |
| sellings_page | `sellings/page-N` (N >= 2) | Sellings.Index | `/sellings/page-1` -> 404 |
| search | `search` | Home.Search (POST only) | `TextSearchTests` |
| info | `info/{alias}` | Info.Page | unpublished/unknown -> 404 |
| articles | `{category}` (NewsCategory titles) | Articles.Index | `/news`, `/overviews`, `/buying-guides`, `/selling-guides`, `/partners` |
| news | `news/{alias}` | Articles.Article | non-normalised alias -> 301 |
| areas | `{area}/{controller=Home}/{action=Index}/{id?}` | Administrator area | |
| default | `{controller=Home}/{action=Index}/{id?}` | | |

## Attribute routes

| Route | Action |
|---|---|
| `GET /identity` | Identity.Index (visitor -> `/?login=true`) |
| `GET /sitemap` | System.Sitemap |
| `GET /sitemap_xml` | System.SitemapXml (application/xml, 15 min cache) |
| `POST /api/stripe/webhook` | Payment.Webhook |
| `GET /admin/run-sellings-job` | Sellings.RunSellingsJob |
| `/Autodata/*`, `/api/autodata/*` | AutodataController (sell-your-car endpoints are anonymous) |

## Key controller routes used by tests

| Route | Method | Auth | Used for |
|---|---|---|---|
| `/` | GET | - | home |
| `/sellings`, `/sellings/{filter}` | GET | - | listings, collections |
| `/Sellings/IndexListing` | POST (AJAX) | - | filter refresh (observed via stale result count) |
| `/System/GetCarModels` | POST | - | model dropdowns (hero, filters) |
| `/System/GetSellingSearchCount` | POST | - | hero count |
| `/System/SearchSuggestions` | POST | - | `TextSearchTests` |
| `/Payment/PaymentDetails?sellingId=` | GET | - | offer page |
| `/Payment/ValidateOffer` | POST | - | offer validation (read-only) |
| `/Payment/ConfirmPayment` | POST | client | offer submit (Guarded) |
| `/personal/sell` | GET | - | sell wizard |
| `/Personal/GetSellCarListingQualification` | POST | client | step 6 -> 7/8 |
| `/Personal/Sell` | POST | client | listing submit (Guarded) |
| `/personal/buying`, `/selling`, `/favorites`, `/settings`, `/notifications`, `/payments` | GET | client | personal area |
| `/Identity/AuthStatus` | GET | - | manual-login detection |
| `/Identity/Authorization` | GET/POST | - | admin login (manual) |
| `/administrator/{module}` | GET | admin/manager | admin suites |
| `/System/NotFoundPage` | - | - | 404 page |
