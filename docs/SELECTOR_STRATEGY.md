# Selector strategy

Selectors were taken from the release-0079 Razor views and scripts, and cross-checked with the selectors that
carabia-ui-tests already runs successfully against DEV.

## Order of preference

1. Stable `id` - e.g. `#crbHeroMakeSelect`, `#sellCarVinInput`, `#pug-file-input`, `#offerInput`, `#lpsLeafletMap`.
2. `data-*` attributes - `[data-filter-key]`, `[data-step-back='3']`, `[data-sell-car-target-step='4']`,
   `input[name='boost_plan'][data-no-boost='true']`, `[data-action='inspection']`, `[data-for-dropdown='personal']`.
3. `name` - filter dropdowns `filters_*`, `crb_sort`, profile inputs `FirstName`, admin form fields (`asp-for` names).
4. ARIA - `a[aria-label='Notifications']`, `#profileBtn[aria-label='Sign in']`.
5. Stable BEM-style classes - `.listing-card__title-link`, `.lps-spec-item`, `.crb-sitemap-col-title`, `.tabs__label_state_active`.
6. Relative XPath only where the markup has no hook: `ancestor::label[1]` for styled radios,
   `ancestor::section[contains(@class,'crb-filter-section')]`, admin rows by exact cell text.

No absolute XPath (`/html/body/...`) anywhere.

## Where selectors live

- `src/Carabia.Automation.Framework/Pages/**` and `Components/**` as `private static readonly By` fields.
- Each page object's XML comment names the controller action and view it was derived from.
- Test classes contain no `By.*` selectors at all (checked: `grep -rn "By\." tests` finds none). Generic audits
  (internal links, images, main content) are `PageBase` methods.

## Behaviour hooks instead of sleeps

| Situation | Signal waited for |
|---|---|
| Filter AJAX refresh | old `.crb-results-count` element becomes stale |
| Dependent dropdowns (Autodata) | select enabled with > 1 option and no "Loading" option |
| Sell wizard steps | `#sell-car-step-N` displayed; `#sellCarLookupMessage` text fails fast |
| Google Places | visible `.pac-item` containing the city, then hidden `#sellCarLocationCountry` filled |
| Photo previews | every `#pug-grid .pug-cell > img` complete with naturalWidth > 0 |
| Modals | `#contactOwnerModal.is-open`, `#modalTDOverlay.active`, `#crbMobileDrawer.crb-active-state`, `.fancybox__container` |
| Manual login | `/Identity/AuthStatus` JSON |
| Navigation | URL predicate or old `<html>` stale |
| Admin jQuery | `jQuery.active === 0` |

## When the UI changes

Update the selector in the one page object/component; the test methods stay unchanged. The admin module list is
data in `AdminModuleCatalog` - adding a controller there automatically adds its list/table/form/tab tests, and
`Catalog_Covers_Every_Administrator_Controller` fails until the catalog matches the source.
