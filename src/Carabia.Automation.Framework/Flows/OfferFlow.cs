using Carabia.Automation.Framework.Base;
using Carabia.Automation.Framework.Models.Results;
using Carabia.Automation.Framework.Pages.Offers;
using Carabia.Automation.Framework.Pages.Personal;
using Carabia.Automation.Framework.Utilities;

namespace Carabia.Automation.Framework.Flows;

/// <summary>
/// Offer negotiation between the Buyer and Seller test accounts on an automation-owned listing.
/// Every method that writes is called only after SafetyGuard allowed OfferWrite.
/// </summary>
public sealed class OfferFlow(BrowserSession session) : FlowBase(session)
{
    /// <summary>Buyer: opens the offer page, enters askingPrice x ratio, submits (WRITE) and waits for "Offer submitted".</summary>
    public OfferResult BuyerSubmitsOffer(int sellingId, decimal ratio)
    {
        var page = new MakeOfferPage(Session);
        page.OpenForSelling(sellingId);
        page.WaitUntilLoaded();
        var amount = decimal.Round(page.AskingPriceValue * ratio);
        page.EnterOffer(amount);
        if (!page.WaitForSubmitState(true))
            throw new AutomationException($"Submit Offer stayed disabled for {amount}: {page.ValidationText}");
        page.ClickSubmitOffer();
        var title = page.WaitForOfferSubmitted();
        return new OfferResult { SellingId = sellingId, Amount = amount, Submitted = true, Message = title };
    }

    /// <summary>Seller: opens My Garage > Selling and the "Manage offer" dialog of the listing.</summary>
    public ListingManagementPage SellerOpensOffer(int sellingId)
    {
        var garage = new MyGaragePage(Session);
        garage.OpenTab(MyGaragePage.GarageTab.Selling);
        var card = garage.CardForSelling(sellingId)
                   ?? throw new AutomationException($"Listing {sellingId} is not in the Seller's garage. Use a listing owned by the Seller test account.");
        if (!garage.CardHasManageOffer(card))
            throw new AutomationException($"Listing {sellingId} has no active offer to manage.");
        garage.OpenManageOffer(card);
        var dialog = new ListingManagementPage(Session);
        dialog.WaitForOfferDialog();
        return dialog;
    }
}
