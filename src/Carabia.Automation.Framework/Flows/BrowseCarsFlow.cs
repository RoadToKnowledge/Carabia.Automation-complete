using Carabia.Automation.Framework.Base;
using Carabia.Automation.Framework.Pages.Public;
using Carabia.Automation.Framework.Utilities;

namespace Carabia.Automation.Framework.Flows;

/// <summary>/sellings -> a listing card -> listing details. Used by listing, gallery, offer and payment tests.</summary>
public sealed class BrowseCarsFlow(BrowserSession session) : FlowBase(session)
{
    /// <summary>Opens /sellings, clicks the n-th card (1-based) and returns the card title and target URL.</summary>
    public (string CardTitle, string Url) OpenListingFromResults(int position = 1)
    {
        var search = new SearchPage(Session);
        search.Open();
        var cards = search.WaitForCards();
        if (cards.Count < position)
            throw new AutomationException($"Expected at least {position} listings on /sellings but found {cards.Count}.");
        var card = cards[position - 1];
        var title = card.Title;
        var url = card.Href;
        card.Open();
        Session.WaitFor("the selected listing's details page to open", d => UrlHelper.SamePath(d.Url, url), Session.LongTimeout);
        new ListingDetailsPage(Session).WaitUntilLoaded();
        return (title, url);
    }

    /// <summary>Returns the URL of the first listing on /sellings without leaving the page state dirty.</summary>
    public string FirstListingUrl()
    {
        var search = new SearchPage(Session);
        search.Open();
        return search.WaitForCards()[0].Href;
    }

    /// <summary>Id of the first active listing (all cards on /sellings are active listings).</summary>
    public int FirstActiveSellingId() =>
        UrlHelper.SellingIdFromUrl(FirstListingUrl())
        ?? throw new AutomationException("Could not read a listing id from the first /sellings card.");
}
