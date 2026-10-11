# Test coverage matrix

Status meanings:
- **DONE** - implemented with source-derived selectors and real assertions; compiles; discovered by NUnit.
- **GUARDED** - implemented, but it writes data, so it runs only when the matching `Allow*Tests` switch is on (DEV only).
- **PARTIAL** - implemented for what can be checked safely; the remainder is listed in "Reason".
- **MANUAL** - authentication: intentionally excluded from Selenium automation.
- **BLOCKED** - needs something that is not available (test Stripe keys, data, feature disabled).
- **N/A** - not a UI feature or intentionally out of scope.

Important: these tests were compiled and discovered but **not executed against dev.carabiacars.com** from the build
machine (no network route to the site). Selectors come from the release-0079 source and from carabia-ui-tests,
which was verified on DEV. First DEV run results belong in `IMPLEMENTATION_PROGRESS.md`.

Abbreviations: PO = page object/component, F = flow.

## Authentication (Identity)

| Source area | Controller.Action | View | Route | JS | Feature | Role | PO / F | Test class.method | Type | Auto? | Reason | Status |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| Identity | SendPhoneOtp / VerifyPhoneOtp / CompleteSignup | _ModelLoginPartial | POST /Identity/* | auth-popup.js | Phone + OTP login, sign-up | Buyer/Seller | ManualAuthenticationManager | *SessionSetupTests (person logs in) | UI | MANUAL | Authentication intentionally excluded from Selenium automation. | MANUAL |
| Identity | Authorization (POST) | Identity/Authorization | /Identity/Authorization | - | Admin username/password | Admin | ManualAuthenticationManager | AdministratorSessionSetupTests | UI | MANUAL | Authentication intentionally excluded from Selenium automation. | MANUAL |
| Identity | GoogleLogin/AppleLogin/*Response, VerifySocialOtp | - | /Identity/* | - | Social login | Client | - | - | - | MANUAL | Authentication intentionally excluded from Selenium automation. | MANUAL |
| Identity | Logout | - | /Identity/Logout | - | Logout | All | - (refused in HeaderComponent/PersonalPageBase) | - | - | MANUAL | Logout is never automated; fresh browser per fixture instead. | MANUAL |
| Identity | ConfirmEmail, SendMail, Verify, ResendEmailOtp, Check*Exists | - | POST | auth-popup.js | E-mail/recovery verification | Client | - | - | - | MANUAL | Authentication intentionally excluded from Selenium automation. | MANUAL |
| Identity | AuthStatus | - | GET /Identity/AuthStatus | - | Auth state | All | AuthStatusProbe | AnonymousAccessTests.Visitor_Is_Reported_Anonymous_By_AuthStatus, *SmokeTests, *SessionSetupTests | API in browser | Automated | Used to detect manual login | DONE |
| Identity | Index | - | /identity | - | Visitor redirect to login | Public | - | AnonymousAccessTests.Identity_Route_Redirects_Visitor_To_Home_With_Login_Prompt | UI | Automated | | DONE |
| Shared | - | _ModelLoginPartial | / (popup) | auth-popup.js | Login popup opens, country selector, Continue enablement (never clicked) | Public | LoginPopupComponent | AnonymousAccessTests.Sign_In_*, Login_Popup_Country_*, Login_Continue_* | UI | Automated | No OTP requested | DONE |
| Personal | [Authorize client] | - | /personal/* | - | Visitor blocked from personal area | Public | LoginPopupComponent | AnonymousAccessTests.Personal_Page_Shows_Login_Instead_Of_Private_Content_For_Visitor (6 routes) | UI | Automated | | DONE |
| Administrator | [Authorize admin/manager] | - | /administrator/* | - | Visitor and client blocked from admin | Public/Buyer | - | AnonymousAccessTests.Administrator_Area_*; AdminAccessForClientTests | UI | Automated | | DONE |
| Framework | - | - | - | - | Saved session create/reuse/expiry | All roles | SessionManager, CookieStorage | *SessionSetupTests | Framework | Automated (after manual login) | | DONE |

## Home, header, footer, navigation

| Source area | Controller.Action | View | Route | JS | Feature | Role | PO / F | Test class.method | Type | Auto? | Reason | Status |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| Home | Index | Home/Index | / | inline hero | Hero count = active cars | Public | HomePage | HomeTests.Hero_Search_Count_Matches_Initial_Active_Car_Count | UI | Automated | | DONE |
| Home | Index | Home/Index | / | GetCarModels, GetSellingSearchCount | Make -> models -> brand page | Public | HomePage | HomeTests.Choosing_A_Make_* | UI | Automated | | DONE |
| Home | Index | Home/Index | / | inline hero | Model search -> /sellings/model-x | Public | HomePage | HomeTests.Choosing_A_Model_* | UI | Automated | | DONE |
| Home | Index | Home/Index | / | inline hero | Empty search -> /sellings | Public | HomePage | HomeTests.Search_Without_Selection_Opens_All_Listings | UI | Automated | | DONE |
| Home | Index | Home/Index | / | - | Quick filter badges | Public | HomePage | HomeTests.Quick_Filter_Badges_*, Clicking_A_Quick_Filter_* | UI | Automated | | DONE |
| Home | Index | Home/Index | / | - | Sell for free tab | Public | HomePage | HomeTests.Sell_For_Free_Tab_Opens_Sell_My_Car | UI | Automated | | DONE |
| Home | Index | Cards/Selling | / | - | Featured listings | Public | HomePage, CarCardComponent | HomeTests.Featured_Listings_*, Configured_Featured_Listing_* | UI | Automated | Featured id is data (Inconclusive if gone) | DONE |
| Home | Index | Faq component | / | accordion | FAQ open/close | Public | FaqComponent | FaqTests.Home_Page_Faqs_Open_And_Close | UI | Automated | | DONE |
| Shared | - | _HeaderPartial | all | - | Visitor header (logo, Sell, Sign in only) | Public | HeaderComponent | NavigationTests.Visitor_Header_*, Header_Sell_My_Car_*, Header_Logo_* | UI | Automated | | DONE |
| Shared | Menus VC | Components/Menus | all | - | Desktop menu links respond | Public | HeaderComponent | NavigationTests.Desktop_Menu_Links_Are_Local_And_Respond | UI+HTTP | Automated | | DONE |
| Shared | - | _HeaderPartial, _Layout drawer script | all | drawer script | Mobile drawer open/close/overlay/sections/links | Public | MobileNavigationComponent | NavigationTests.Mobile_Drawer_* (4) | UI | Automated | | DONE |
| Shared | - | _HeaderPartial | all | dropdown | Signed-in header, personal menu, bell | Buyer | HeaderComponent | BuyerPersonalAreaTests.Signed_In_Header_*, Personal_Menu_*, Header_Bell_* | UI | Automated | Log out item never clicked | DONE |
| Shared | - | _FooterPartial | all | - | Footer internal links (14) | Public | FooterComponent | FooterTests.Footer_Internal_Link_Opens_Expected_Page | UI+HTTP | Automated | | DONE |
| Shared | - | _FooterPartial | all | - | Footer link hygiene | Public | FooterComponent | FooterTests.Every_Footer_Link_Has_A_Usable_Destination | UI | Automated | | DONE |
| Shared | - | _FooterPartial | all | - | Contact/social links (5) | Public | FooterComponent | FooterTests.Footer_Contact_And_Social_Links_* | UI | Automated | External sites not opened | DONE |
| Shared | - | _SubscribePartial | several | - | Newsletter subscribe (AddSubscribeEmail) | Public | - | - | - | N/A | Would add real contacts to the mailing service | N/A |
| Home | Join | MailTemplates/PrelaunchJoinUs | /home/join | - | Pre-launch join link | Public | - | - | - | N/A | Needs e-mailed token | N/A |

## Search, collections, listings

| Source area | Controller.Action | View | Route | JS | Feature | Role | PO / F | Test class.method | Type | Auto? | Reason | Status |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| Sellings | Index/IndexListing | _FiltersPartial | /sellings | filters | Make filter | Public | FilterComponent | FilterTests.Selecting_A_Make_* | UI | Automated | Needs Audi/BMW listings | DONE |
| Sellings | Index/IndexListing | _FiltersPartial | /sellings | GetCarModels | Model filter, make change resets model | Public | FilterComponent | FilterTests.Selecting_A_Model_*, Changing_Make_* | UI | Automated | carabia-ui-tests saw HTTP 500 on GetCarModels (see KNOWN_LIMITATIONS) | DONE |
| Sellings | Index/IndexListing | _FiltersPartial | /sellings | filters | Price/year/mileage ranges, combined | Public | FilterComponent | FilterTests.Numeric_Range_* (3), Make_And_Price_* | UI | Automated | | DONE |
| Sellings | Index/IndexListing | _FiltersPartial | /sellings | filters | Clear all | Public | FilterComponent | FilterTests.Clear_All_* | UI | Automated | known max-price reset issue | DONE |
| Sellings | IndexListing | _NoListingResultsPartial | /sellings | - | Empty results panel | Public | FilterComponent | FilterTests.Impossible_Filter_Combination_* | UI | Automated | | DONE |
| Sellings | Index/IndexListing | _FiltersPartial | /sellings | crb_sort | Sorting (3) + default | Public | FilterComponent | SortingTests.* | UI | Automated | | DONE |
| Sellings | Index | _IndexListingPartial | /sellings/page-N | pagination | Pagination | Public | PaginationComponent | PaginationTests.* (4) | UI+HTTP | Automated | Inconclusive if one page | DONE |
| Sellings | Index | Sellings/Index | /sellings/{quick} | - | 8 quick-filter collections | Public | CollectionPage | CollectionPageTests.Quick_Filter_Collection_* | UI | Automated | | DONE |
| Sellings | Index | Sellings/Index | /sellings/{body-,location-,model-,under-} | - | Prefix collections + content rules | Public | CollectionPage | CollectionPageTests.Prefix_*, Under_Price_*, Model_Collection_*, Collection_Cards_* | UI | Automated | data dependent values in testdata | DONE |
| Sellings | Index (status) | Sellings/Index | /sellings/sold | - | Previously listed | Public | CollectionPage | CollectionPageTests.Previously_Listed_Status_Page_Opens | UI | Automated | | DONE |
| Home | Brand | Home/Brand | /brand/{brand} | - | Brand pages + FAQ | Public | BrandPage, FaqComponent | FaqTests.Make_Or_Model_Landing_*, HomeTests | UI | Automated | | DONE |
| Home | Search | Home/Search | POST /search | - | Text search | Public | HttpProbe | TextSearchTests.* | HTTP | Automated | no search box in current header | DONE |
| Sellings | Index | Cards/Selling | /sellings | lazy images | Card content, images, card -> details (4) | Public | CarCardComponent | ListingCardTests.* | UI | Automated | | DONE |
| Sellings | Selling | Sellings/Selling | /sellings/{id}_{model} | - | Specs, heading, price, finance, seller, VIN field | Public | ListingDetailsPage, FeaturesComponent | ListingDetailsTests.* | UI | Automated | | DONE |
| Sellings | Selling | Sellings/Selling | " | Fancybox 4 | Main image, counter, thumbnails, Photo Gallery, full-screen | Public | GalleryComponent | GalleryTests.* (5) | UI | Automated | "+N more" overlay is commented out in source | DONE |
| Sellings | Selling | Sellings/Selling | " | features toggle | Feature icons, see more | Public | FeaturesComponent | FeatureAndLocationTests.Feature_*, See_More_* | UI | Automated | | DONE |
| Sellings | Selling | Sellings/Selling | " | Leaflet | Map, zoom levels | Public | LocationMapComponent | FeatureAndLocationTests.Location_*, Map_Zoom_* | UI | Automated | | DONE |
| Sellings | Selling | Sellings/Selling | " | contact owner script | Contact the Owner dialog, Escape, Make an Offer link | Public | OfferComponent | ContactOwnerAndServicesTests.Contact_*, Make_An_Offer_* | UI | Automated | | DONE |
| Sellings | Selling / PostTestDriveRequest | Sellings/Selling | " | test-drive.js, auth-popup.js | Test drive: visitor -> login; client -> sheet opens | Public/Buyer | OfferComponent | ContactOwnerAndServicesTests.Visitor_Requesting_A_Test_Drive_*, BuyerListingInteractionTests.Buyer_Can_Open_* | UI | PARTIAL | Request not submitted (notifies a real seller) | PARTIAL |
| Sellings / System | Selling / AddInspectionReportRequest | Sellings/Selling | " | auth-popup.js | Additional services, inspection dialog | Public/Buyer | AdditionalServicesComponent | ContactOwnerAndServicesTests.Additional_*, Visitor_Requesting_Inspection_*, BuyerListingInteractionTests.*Inspection* | UI | PARTIAL | "Send" creates a service request; not clicked | PARTIAL |
| Sellings | Selling | Sellings/Selling | " | custom.js | Favourites (visitor -> login; client cookie) | Public/Buyer | ListingDetailsPage | ListingDetailsTests.Visitor_Wishlist_*, BuyerPersonalAreaTests.Adding_And_Removing_A_Favourite_* | UI | Automated | cookie only, restored | DONE |
| Sellings | Selling | Sellings/Selling | " | accordion | Listing FAQs | Public | FaqComponent | ListingDetailsTests.Listing_Faqs_Open_And_Close | UI | Automated | | DONE |
| System | AddComment/AddCommentReply/AddCommentLike | Sellings/Selling | POST | custom.js | Comments | Client | - | ListingDetailsTests.Ask_A_Question_* (presence only) | UI | PARTIAL | Posting would publish real comments | PARTIAL |
| Sellings | BuyNow, MakeOffer, GetBuyersPremium | - | POST | - | Legacy buy-now/offer endpoints | Client | - | - | - | N/A | Not reachable from current UI (offer goes through PaymentDetails) | N/A |
| Sellings | Selling | - | /sellings/{id}_{wrong} | - | Wrong slug 404 | Public | HttpProbe | ListingDetailsTests.Listing_Url_With_Wrong_Model_Slug_Returns_404 | HTTP | Automated | | DONE |

## Sell My Car

| Source area | Controller.Action | View | Route | JS | Feature | Role | PO / F | Test class.method | Type | Auto? | Reason | Status |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| Personal | Sell (GET) | Personal/Sell #step-1 | /personal/sell | syncSellCarVehicleInputs | Empty step blocked, Year list | Public | SellCarPage, VehicleSelectorComponent | SellValidationTests.Empty_Vehicle_Step_Cannot_Continue | UI | Automated | | DONE |
| Personal | Sell | step 1 | " | VIN length check | Invalid VIN (2) | Public | VehicleSelectorComponent | SellValidationTests.Vin_With_Wrong_Length_* | UI | Automated | | DONE |
| Personal | Sell | step 1 | " | syncSellCarVehicleInputs | VIN/manual exclusivity both ways | Public | VehicleSelectorComponent | SellValidationTests.Typing_A_Vin_Disables_*, Vin_Takes_Priority_* | UI | Automated | | DONE |
| Autodata | sell-your-car makes/models/specifications | step 1 | /api/autodata/sell-your-car/* | dependent dropdowns | Dependent dropdowns, Next waits for regional spec, year change resets | Public | VehicleSelectorComponent | SellValidationTests.Dependent_*, Changing_Year_* | UI | Automated | | DONE |
| Personal | Sell | step 1 | " | - | VIN help | Public | VehicleSelectorComponent | SellValidationTests.Vin_Help_* | UI | Automated | | DONE |
| Autodata | decode-vin / manual-lookup | steps 1-3 | " | wizard | VIN path and manual path to features | Public | SellCarFlow | VehicleLookupTests.Vin_Lookup_*, Manual_Selection_* | UI | Automated | needs Autodata + Maps on DEV | DONE |
| Personal | Sell | step 2 | " | Google Places | Location resolves city/country/coords | Public | LocationSelectorComponent | VehicleLookupTests.Google_Places_* | UI | Automated | | DONE |
| Personal | Sell | steps 2-3 | " | data-step-back | Back/next keeps data | Public | SellWizardComponent | VehicleLookupTests.Back_Buttons_*, SellerSellFlowTests.Summary_Back_* | UI | Automated | | DONE |
| Personal | Sell | step 3 | " | notes popup | Features/condition notes | Public | SellCarPage | VehicleLookupTests.Owner_Notes_* | UI | PARTIAL | Damage report and feature chips not individually asserted | PARTIAL |
| Personal | Sell | step 4 | " | price selector | Asking price | Public | PriceSelectorComponent | VehicleLookupTests.Asking_Price_* | UI | Automated | | DONE |
| Personal | Sell | step 5 | " | PUG uploader | Min 4 photos, previews, cover | Seller | ImageUploadComponent | SellerSellFlowTests.Photo_Step_*, Uploaded_Photos_* | UI | Automated | | DONE |
| Personal | Sell | step 5 | " | pugHandleUpload | Invalid file rejected | Seller | ImageUploadComponent | SellerSellFlowTests.Non_Image_File_Is_Not_Accepted_As_A_Photo | UI | Automated (KnownIssue) | App accepts any file type - expected to fail until fixed | DONE |
| Personal | GetSellCarListingQualification | steps 6-8 | POST | wizard | Ownership skip, plan, boost | Seller | SellCarPage | SellerSellFlowTests.Flow_Reaches_Summary_* | UI | Automated | Mulkiya upload not automated (real documents) | DONE |
| Personal | Sell | step 9 | " | summary | Summary values, both paths, stop before submit | Seller | SellCarFlow | SellerSellFlowTests.Vin_Flow_Reaches_Summary_*, Manual_Flow_Reaches_Summary_* | UI | Automated | | DONE |
| Personal | Sell (POST) | step 11 | POST /Personal/Sell | submitSellCarListing | Submit free listing + appears in Submissions | Seller | SellCarPage | SellerSellFlowTests.Seller_Can_Submit_A_Free_Listing_* | UI | GUARDED | AllowSellSubmissionTests | GUARDED |
| Payment | CreateSellListingPaymentIntent | step 10 | POST | sell-car-payment-stripe.js | Paid boost -> payment step, card element, Pay disabled | Seller | StripePaymentPage | SellListingPaymentTests.Paid_Boost_Opens_Payment_Step_* | UI | Automated | no card typed | DONE |
| Payment | CreateSellListingPaymentIntent / Cancel | step 10 | POST | Stripe | Declined / successful TEST card | Seller | StripePaymentPage | SellListingPaymentTests.Declined_*, Successful_* | UI | GUARDED + BLOCKED | Needs pk_test_ key; release uses pk_live_ | BLOCKED |

## Offers and payments

| Source area | Controller.Action | View | Route | JS | Feature | Role | PO / F | Test class.method | Type | Auto? | Reason | Status |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| Payment | PaymentDetails | Payment/PaymentDetails | /Payment/PaymentDetails | payment.js | Offer page content, disabled submit | Public | MakeOfferPage | MakeOfferPageTests.Offer_Page_Shows_* | UI | Automated | | DONE |
| Payment | ValidateOffer | " | " | payment.js | 75 % minimum, too-low error, valid enables submit | Public | MakeOfferPage | MakeOfferPageTests.Minimum_*, Offer_Below_*, Valid_Offer_* | UI | Automated | | DONE |
| Payment | ValidateOffer | " | " | payment.js, auth-popup.js | Visitor submit -> login | Public | MakeOfferPage | MakeOfferPageTests.Visitor_Submitting_* | UI | Automated | | DONE |
| Payment | PaymentDetails | " | " | - | Stripe key mode report (never printed) | Public | StripeModeInspector | MakeOfferPageTests.Offer_Page_Reports_Stripe_Mode_* | UI | Automated | | DONE |
| Payment | PaymentDetails | " | " | - | Card screen not shown up front; anonymous write endpoints fail | Public | MakeOfferPage, HttpProbe | PaymentPageSafetyTests.* | UI+HTTP | Automated | | DONE |
| Payment | ConfirmPayment | " | POST | submitZeroPaymentOffer | Buyer submits offer, sees it under Buying, low 2nd offer rejected | Buyer | OfferFlow | BuyerOfferTests.* | UI | GUARDED | AllowOfferWriteTests + AutomationSellingId | GUARDED |
| Personal | (canInteract) | Sellings/Selling | - | - | Owner cannot offer on own listing | Seller | ListingDetailsPage | SellerOfferResponseTests.Seller_Cannot_Make_An_Offer_On_Own_Listing | UI | GUARDED | needs automation listing id | GUARDED |
| Personal | GetActiveOfferInfo / MakeOffer / DeclineOffer / AcceptOffer | _ModalPartial #modalOffer | POST | garage.js | Seller sees offer, counter, decline, accept | Seller | OfferFlow, ListingManagementPage | SellerOfferResponseTests.* | UI | GUARDED | Accept also needs AllowPaymentWriteTests (may capture) | GUARDED |
| Personal | Payments | Personal/Payments | /personal/payments | - | Payment history table | Buyer | PersonalPaymentsPage | BuyerPersonalAreaTests.Payments_Page_* | UI | Automated | title bug tracked as KnownIssue | DONE |
| Personal | BindCard, SetActiveCard | Personal/Settings | - | - | Saved cards | Client | - | - | - | N/A | Real card binding (Telr/Stripe) - not safe | N/A |
| Payment | Webhook | - | /api/stripe/webhook | - | Stripe events | Stripe | - | - | - | N/A | Server-to-server | N/A |

## Personal area

| Source area | Controller.Action | View | Route | JS | Feature | Role | PO / F | Test class.method | Type | Auto? | Reason | Status |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| Personal | Buying/Selling/Favorites | Personal/Garage | /personal/buying... | - | Tabs, breadcrumb, active nav | Buyer | MyGaragePage | BuyerPersonalAreaTests.My_Garage_* | UI | Automated | | DONE |
| Personal | - | _NavigationPartial | - | - | Side navigation; logout present, never used | Buyer | PersonalPageBase | BuyerPersonalAreaTests.Personal_Navigation_* | UI | Automated | | DONE |
| Personal | Selling | Garage + _GarageSellingPartial | /personal/selling | - | Status groups, cards, View Listing links | Seller | MyGaragePage | SellerPersonalAreaTests.Selling_Tab_*, Status_Group_*, Listing_Card_* | UI | Automated | Inconclusive without listings | DONE |
| Personal | EditPrice | #modalEditPrice | POST | garage.js | Edit price dialog / save | Seller | ListingManagementPage | SellerPersonalAreaTests.Edit_Price_Dialog_*, Seller_Can_Change_Price_* | UI | Automated + GUARDED | | DONE |
| Personal | AddCarDeletionRequest | #modalRemoveListing | POST | garage.js | Removal request dialog | Seller | ListingManagementPage | SellerPersonalAreaTests.Remove_Listing_Dialog_* | UI | PARTIAL | Request never sent (creates admin work item) | PARTIAL |
| Personal | Settings | Personal/Settings | /personal/settings | settings script | Read-only fields, edit unlock, validation, e-mail verification gate, reload discard | Buyer | ProfilePage | ProfileTests.* (5) | UI | Automated | | DONE |
| Personal | Settings (POST) | " | POST | - | Save names + restore | Buyer | ProfilePage | ProfileTests.Saved_Names_Persist_* | UI | GUARDED | AllowProfileWriteTests | GUARDED |
| Personal | SendEmailConfirmation | " | POST | - | E-mail code | Client | - | - | - | MANUAL | Authentication-like verification (sends e-mail) | MANUAL |
| Personal | Notifications | Personal/Notifications | /personal/notifications | - | Log, grouping | Buyer | NotificationsPage | BuyerPersonalAreaTests.Notifications_Page_Shows_Log | UI | Automated | GET marks notifications as shown (normal user effect) | DONE |

## Auctions, articles, info, system

| Source area | Controller.Action | View | Route | JS | Feature | Role | PO / F | Test class.method | Type | Auto? | Reason | Status |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| Auctions | Index | Auctions/Index | /auctions | - | Coming soon state | Public | AuctionsPage | AuctionTests.Auctions_Page_Shows_* | UI | Automated | | DONE |
| Auctions | Index | Auctions/Index | /auctions?auctions_visible | auctions.js | Hidden grid + filters | Public | AuctionsPage | AuctionTests.Auctions_Visible_* | UI | Automated | | DONE |
| Auctions | Index(status) | - | /auctions/sold, /upcoming | - | Redirect | Public | HttpProbe | AuctionTests.Auction_Status_Pages_Redirect_* | HTTP | Automated | | DONE |
| Auctions | Auction | Auctions/Auction | /auctions/{id}_{m} | - | Detail | Public | AuctionDetailsPage | AuctionTests.Auction_Detail_* | UI | Automated | Inconclusive without auctions | DONE |
| Auctions | AddBid | Auctions/Auction | POST | auctions.js | Bidding | Client | - | - | - | BLOCKED | Auctions disabled in this release; bids are binding | BLOCKED |
| Articles | Index | Articles/Index | /{category} | - | 5 categories, category buttons | Public | ArticlesPage | ArticleTests.Article_Category_* | UI+HTTP | Automated | | DONE |
| Articles | Article | Articles/Article | /news/{alias} | - | Article page, 301 normalisation, 404 | Public | ArticleDetailsPage | ArticleTests.Opening_*, Article_Alias_*, Unknown_Article_* | UI+HTTP | Automated | | DONE |
| Info | Page | Info/Page | /info/{alias} | fees anchor | CMS pages (5), 404, fees anchor, contact page | Public | InfoPage, ContactPage | StaticPageTests.* | UI+HTTP | Automated | | DONE |
| System | NotFoundPage | System/NotFoundPage | any unknown | - | 404 status + page + link home | Public | ErrorPage | ErrorPageTests.* | UI+HTTP | Automated | | DONE |
| System | Sitemap | System/Sitemap | /sitemap | - | HTML sitemap | Public | SitemapPage | SitemapTests.Html_Sitemap_*; BrokenLinkTests (/sitemap) | UI | Automated | | DONE |
| System | SitemapXml | SitemapXml | /sitemap_xml | - | XML validity, collections listed, sampled URLs respond | Public | HttpProbe | SitemapTests.Sitemap_Xml_* (3) | HTTP | Automated | | DONE |
| Shared | - | _SitemapLinksPartial | /, /sellings, /brand/bmw | - | 6 groups, link checks, click-through | Public | SitemapLinksComponent | SitemapTests.Shared_*, Every_Link_*, Clicking_* | UI+HTTP | Automated | | DONE |
| Middleware | CanonicalMiddleware | - | any | - | 301 lower case, trailing slash, query kept | Public | HttpProbe | CanonicalAndRedirectTests.Upper_*, Trailing_*, Query_* | HTTP | Automated | | DONE |
| Views | - | Home/Index, Sellings/*, Auctions/Index, Info/Page | - | - | rel=canonical, title, meta description, h1 | Public | PageBase | CanonicalAndRedirectTests.Page_Has_*, Listing_Detail_Canonical_* | UI | Automated | | DONE |
| Middleware | RedirectsMiddleware (redirects.txt) | - | legacy URLs | - | Legacy redirects | Public | - | - | - | N/A | Inventory-specific URLs; not asserted | N/A |
| All | - | - | /, /sellings, /sitemap | - | Broken internal links, home images | Public | HttpProbe | BrokenLinkTests.* | HTTP+UI | Automated | | DONE |
| Responsive | - | layouts | 5 pages x 3 viewports | - | No horizontal scroll, header/drawer, hero, cards, details, sell step 1 | Public | Viewport, WindowManager | ResponsiveTests.* | UI | Automated | | DONE |

## Administrator area (every controller)

| Module | Actions in source | Route | Role | PO | Tests | Auto? | Reason | Status |
|---|---|---|---|---|---|---|---|---|
| Sellings | Index, Edit, AddPayment, RemoveBuyer, Remove | /administrator/sellings | admin, manager | AdminPage, AdminModuleCatalog | AdminModuleTests (layout, table, remove guard, tabs x5, first edit page); AdminSmokeTests | PARTIAL | Edit save, payments, remove buyer/listing not automated (real customer data) | PARTIAL |
| SellingOffers | Index (filter), Details, Accept, Decline | /administrator/sellingoffers | admin, manager | SellingOffersPage | AdminModuleTests; AdminWorkflowTests.Selling_Offers_Status_Filter_*, Selling_Offer_Details_* | PARTIAL | Accept/Decline may capture/cancel payments | PARTIAL |
| Auctions | Index, Edit, ChangeWinner, AddPayment, Remove | /administrator/auctions | admin, manager | AdminPage | AdminModuleTests | PARTIAL | writes not automated | PARTIAL |
| Moderation | Next, Moderate, ModerateSelling/Auction, Decline* | /administrator/moderation/next | admin, manager | ModerationPage | AdminModuleTests; AdminWorkflowTests.Moderation_Queue_* | PARTIAL | Approving/declining real submissions is out of scope; automation listings could be moderated manually | PARTIAL |
| Dictionary | Index, EditBodyType/Color/Location, Remove* | /administrator/dictionary | admin | AdminPage, AdminEditFormPage | AdminModuleTests (3 add forms, validation) | PARTIAL | Saving changes public filters | PARTIAL |
| Manufacturers | Index, Edit, Remove | /administrator/manufacturers | admin | " | AdminModuleTests | PARTIAL | " | PARTIAL |
| Models | Index, Edit, Remove | /administrator/models | admin | " | AdminModuleTests | PARTIAL | " | PARTIAL |
| Autodata | Index (+ import endpoints) | /administrator/autodata | admin | AutodataPage | AdminModuleTests; AdminWorkflowTests.Autodata_Page_* | PARTIAL | Imports rewrite catalogue data - never started | PARTIAL |
| Banners | Index, Edit, Remove | /administrator/banners | admin | AdminPage | AdminModuleTests | PARTIAL | | PARTIAL |
| News | Index, Edit, Remove, EditCategory, RemoveCategory | /administrator/news | admin, manager | AdminPage, AdminEditFormPage | AdminModuleTests (2 add forms) | PARTIAL | categories become routes | PARTIAL |
| Pages | Index, Edit, Remove | /administrator/pages | admin | " | AdminModuleTests | PARTIAL | | PARTIAL |
| Faq | Index, Edit, Remove, EditCategory, RemoveCategory | /administrator/faq | admin | FaqAdminPage, AdminContentFlow | AdminModuleTests; AdminWorkflowTests.Admin_Can_Create_Rename_And_Remove_An_Automation_Faq_Category | GUARDED (Destructive) | AUTO-marked row only | GUARDED |
| Clients | Index, Edit, Remove | /administrator/clients | admin, manager | AdminPage | AdminModuleTests | PARTIAL | personal data - read only | PARTIAL |
| Administrators | Index, EditAdministrator, EditManager, Remove* | /administrator/administrators | admin | AdminPage, AdminEditFormPage | AdminModuleTests (2 add forms) | PARTIAL | creating admin accounts is a security action | PARTIAL |
| ServiceRequests | Index, Remove | /administrator/servicerequests | admin | AdminPage | AdminModuleTests (4 sections; unguarded Remove reported as warning) | PARTIAL | | PARTIAL |
| TestDrive | Index, Details, UpdateDetails, status | /administrator/testdrive | admin, manager | AdminPage | AdminModuleTests (tabs); AdminWorkflowTests.Test_Drive_* | PARTIAL | | PARTIAL |
| Payments | Index | /administrator/payments | admin | AdminPage | AdminModuleTests (exact columns) | Automated | read-only module | DONE |
| Comments | Moderation, Moderate | /administrator/comments/moderation | admin, manager | ModerationPage | AdminModuleTests; AdminWorkflowTests.Moderation_Queue_* | PARTIAL | | PARTIAL |
| Cars | RemovePhoto, RemoveDocument | (no page) | admin, manager | - | AdminModuleTests.Catalog_Covers_Every_Administrator_Controller | N/A | destructive GET endpoints only | N/A |
| System | UploadImage | (no page) | admin, manager | - | catalog test | N/A | editor upload endpoint | N/A |
| Navigation | _LeftNav, _TopNavbar | all | admin | AdminPage | AdminWorkflowTests.Left_Navigation_Links_All_Open_Admin_Pages | Automated | Logout link in top bar never used | DONE |

## Totals

421 NUnit test cases discovered (parameterised cases counted individually):
- 265 run without any login (Public, Smoke-public, SEO, Responsive, visitor sell flow, visitor offer page),
- 156 need a manual-auth session (Buyer 35, Seller 24, Admin 99; the two client-blocked admin cases count in both),
- 15 are in WritesData fixtures/methods (guarded by Allow*Tests switches), 1 of them Destructive,
- 2 are tagged KnownIssue (they document current application bugs and are expected to fail until fixed).
