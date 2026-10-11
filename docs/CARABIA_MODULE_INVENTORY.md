# Carabia module inventory (release 0079)

Scope column: **Automated** = covered by tests; **Guarded** = automated but runs only when a safety switch allows writes;
**Manual** = intentionally not automated (authentication); **Not automated** = reason given.

## Public / client modules

| Module | Purpose | Controller | Actions | Views | JS | Routes | Role | Automation scope |
|---|---|---|---|---|---|---|---|---|
| Home | Landing, hero search, quick filters, featured cars, FAQ | HomeController | Index, Search (POST), Brand, BrandListing, HeaderPartial, Join | Home/Index, Home/Search, Home/Brand, _BrandListingPartial | inline hero script | `/`, `/search`, `/brand/{brand}...` | Public | Automated (Join: not automated - e-mail token) |
| Listings / search | Results, sidebar filters, sorting, pagination, collections | SellingsController | Index, IndexListing (AJAX) | Sellings/Index, _FiltersPartial, _IndexListingPartial, _BrandSidebarFiltersPartial | filters script | `/sellings`, `/sellings/{filter}`, `/sellings/sold`, `/sellings/page-N` | Public | Automated |
| Listing details | Gallery, specs, features, price, map, contact owner, services, FAQ, comments | SellingsController | Selling, MakeOffer, BuyNow, GetBuyersPremium, PostTestDriveRequest | Sellings/Selling | Selling.cshtml scripts, test-drive.js, auth-popup.js, custom.js | `/sellings/{id}_{model}` | Public / client | Automated; test-drive POST, BuyNow, comment posting: not automated (notify real sellers) |
| Offers | Make offer, review, submit | PaymentController | PaymentDetails, ValidateOffer, ConfirmPayment, CalculateEstimatedMonthlyPayment | Payment/PaymentDetails | payment.js, payment-stripe.js | `/Payment/PaymentDetails?sellingId=` | Public / client | Automated (UI) + Guarded (submit) |
| Seller offer handling | Accept / decline / counter | PersonalController | GetActiveOfferInfo, AcceptOffer, DeclineOffer, MakeOffer | Personal/Garage, _ModalPartial (#modalOffer) | garage.js | POST `/Personal/*` | Seller | Guarded |
| Sell My Car | 11-step listing wizard | PersonalController, AutodataController | Sell (GET/POST), GetSellCarListingQualification, sell-your-car APIs | Personal/Sell | inline wizard, sell-car-payment-stripe.js | `/personal/sell`, `/api/autodata/sell-your-car/*` | Public (steps 1-5), Seller | Automated to summary; submit Guarded |
| Sell payment | Paid plan / boost authorisation | PaymentController | CreateSellListingPaymentIntent, CancelSellListingPaymentIntent | Personal/Sell step 10 | sell-car-payment-stripe.js | POST `/Payment/*` | Seller | UI Automated; card authorisation Guarded (+ pk_test_ required) |
| My Garage | Buying / Selling / Favourites | PersonalController | Buying, Selling, Favorites | Personal/Garage + partials | garage.js, custom.js (favourites cookie) | `/personal/buying|selling|favorites` | Client | Automated |
| Listing management | Edit price, removal request | PersonalController | EditPrice, AddCarDeletionRequest | _ModalPartial (#modalEditPrice, #modalRemoveListing) | garage.js | POST | Seller | Dialogs Automated; price edit Guarded; removal request not automated |
| Settings / profile | Personal info, e-mail verification, cards | PersonalController | Settings (GET/POST), SendEmailConfirmation, SetActiveCard, BindCard | Personal/Settings | settings script | `/personal/settings` | Client | Automated; save Guarded; e-mail code, cards: not automated |
| Notifications | Notification log | PersonalController | Notifications | Personal/Notifications | - | `/personal/notifications` | Client | Automated |
| Payments history | Client payments | PersonalController | Payments | Personal/Payments | - | `/personal/payments` | Client | Automated |
| Auctions | Auction list/detail/bids | AuctionsController | Index, IndexListing, Auction, AddBid | Auctions/Index, Auctions/Auction | auctions.js | `/auctions`, `/auctions/{id}_{model}` | Public / client | Automated (list, hidden grid, redirects, detail if data); bidding not automated (feature disabled) |
| Articles | News categories and articles | ArticlesController | Index, Article | Articles/Index, Articles/Article | - | `/{category}`, `/news/{alias}` | Public | Automated |
| Info pages | CMS pages | InfoController | Page | Info/Page | - | `/info/{alias}` | Public | Automated |
| System | Sitemap, XML sitemap, comments, models, counts, requests, 404 | SystemController | Sitemap, SitemapXml, GetCarModels, GetSellingSearchCount, SearchSuggestions, AddComment*, AddInspectionReportRequest, AddFinanceRequest, AddShippingRequest, AddSubscribeEmail, CheckCardBinding, NotFoundPage | System/Sitemap, System/NotFoundPage | various | `/sitemap`, `/sitemap_xml`, POST `/System/*` | Public / client | Read endpoints Automated; write endpoints not automated (create real requests/comments/subscriptions) |
| Identity | Login, OTP, sign-up, social login, logout | IdentityController | Index, Authorization, SendPhoneOtp, VerifyPhoneOtp, Signup, CompleteSignup, Google*, Apple*, Logout, AuthStatus ... | Identity/*, _ModelLoginPartial | auth-popup.js | `/identity`, `/Identity/*` | All | **Manual** - "Authentication intentionally excluded from Selenium automation." (popup opening and AuthStatus are automated) |
| Autodata API | Vehicle data | AutodataController | 40+ API actions (role-protected or sell-your-car) | - | - | `/api/autodata/*` | Admin / public subset | Exercised through the sell wizard; direct API tests not in UI scope |
| Stripe webhook | Payment events | PaymentController | Webhook | - | - | `/api/stripe/webhook` | Stripe | Not automated (server-to-server) |
| Sellings job | Maintenance endpoint | SellingsController | RunSellingsJob | - | - | `/admin/run-sellings-job` | Ops | Not automated (operational job) |

## Administrator area (`Areas/Administrator`)

| Module | Controller | Actions | Views | Role | Automation scope |
|---|---|---|---|---|---|
| Sellings | SellingsController | Index (5 tabs), Edit GET/POST, AddPayment, RemoveBuyer, Remove | Sellings/Index, Sellings/Edit | admin, manager | List, tabs, edit page opened read-only |
| Selling offers | SellingOffersController | Index (status filter), Details, Accept, Decline | SellingOffers/Index, Details | admin, manager | List, filter, details; Accept/Decline not clicked |
| Auctions | AuctionsController | Index, Edit, ChangeWinner, AddPayment, Remove | Auctions/Index, Edit | admin, manager | List, edit page read-only |
| Moderation ("New offers") | ModerationController | Next, Moderate, ModerateAuction, ModerateSelling, DeclineAuction, DeclineSelling | SellingModeration, AuctionModeration, Empty | admin, manager | Queue state read-only |
| Dictionary | DictionaryController | Index, EditBodyType/Color/Location, Remove* | Dictionary/* | admin | List, add forms (not saved) |
| Manufacturers | ManufacturersController | Index, Edit, Remove | Manufacturers/* | admin | List, add form |
| Models | ModelsController | Index, Edit, Remove | Models/* | admin | List, add form |
| Autodata import | AutodataController | Index | Autodata/Index | admin | Page + buttons; imports never started |
| Banners | BannersController | Index, Edit, Remove | Banners/* | admin | List, edit page read-only |
| News | NewsController | Index, Edit, Remove, EditCategory, RemoveCategory | News/* | admin, manager | List, add forms |
| Pages | PagesController | Index, Edit, Remove | Pages/* | admin | List, add form |
| FAQ | FaqController | Index, Edit, Remove, EditCategory, RemoveCategory | Faq/* | admin | List, add forms, **Guarded/Destructive CRUD** of an AUTO category |
| Clients | ClientsController | Index, Edit, Remove | Clients/* | admin, manager | List, edit page read-only |
| Administrators | AdministratorsController | Index, EditAdministrator, EditManager, Remove* | Administrators/* | admin | List, add forms (never saved - security) |
| Service requests | ServiceRequestsController | Index, Remove | ServiceRequests/Index | admin | List (4 sections) |
| Test drives | TestDriveController | Index (3 tabs), Details, UpdateDetails, status POST | TestDrive/* | admin, manager | List, tabs |
| Payments | PaymentsController | Index | Payments/Index | admin | List + exact columns |
| Comments | CommentsController | Moderation, Moderate | Comments/Moderation, Empty | admin, manager | Queue state read-only |
| Cars | CarsController | RemovePhoto, RemoveDocument | - | admin, manager | Not automated (destructive GET, no page) |
| System | SystemController | UploadImage | - | admin, manager | Not automated (TinyMCE upload endpoint) |
