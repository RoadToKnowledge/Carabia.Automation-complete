using Carabia.Automation.Framework.Pages.Public;
using Carabia.Automation.Framework.Utilities;
using Carabia.Automation.Tests.Base;

namespace Carabia.Automation.Tests.Listings;

/// <summary>Listing cards on /sellings (carabia-ui-tests ListingsPresentationTests).</summary>
[TestFixture]
[Category(TestCategories.Public)]
[Category(TestCategories.Listings)]
[Category(TestCategories.Regression)]
[Parallelizable(ParallelScope.Self)]
public sealed class ListingCardTests : UiTestBase
{
    [Test]
    public void Listings_Page_Shows_Heading_And_A_Result_Count_Not_Below_The_Cards_Shown()
    {
        var page = new SearchPage(Session);
        page.Open();
        var cards = page.WaitForCards();
        Assert.Multiple(() =>
        {
            Assert.That(page.HeadingText, Does.Contain("cars").IgnoreCase);
            Assert.That(page.ResultCount, Is.GreaterThan(0));
            Assert.That(cards.Count, Is.LessThanOrEqualTo(page.ResultCount));
        });
    }

    [Test]
    public void Every_Card_Has_Title_Year_Mileage_Price_And_Details_Link()
    {
        var page = new SearchPage(Session);
        page.Open();
        var errors = new List<string>();
        var cards = page.WaitForCards();
        for (var i = 0; i < cards.Count; i++)
        {
            var card = cards[i];
            var context = $"Card {i + 1}/{cards.Count}";
            if (!card.HasTitle) { errors.Add($"{context}: no title link."); continue; }
            if (string.IsNullOrWhiteSpace(card.Title)) errors.Add($"{context}: empty title.");
            if (string.IsNullOrWhiteSpace(card.MetaText)) errors.Add($"{context} ({card.Title}): no year/mileage.");
            if (!TextParser.TryNumber(card.PriceText, out _)) errors.Add($"{context} ({card.Title}): no numeric price '{card.PriceText}'.");
            if (UrlHelper.SellingIdFromUrl(card.Href) is null) errors.Add($"{context} ({card.Title}): bad details link {card.Href}.");
        }
        Assert.That(errors, Is.Empty, string.Join("\n", errors));
    }

    [Test]
    public void Every_Card_Image_Loads()
    {
        var page = new SearchPage(Session);
        page.Open();
        var cards = page.WaitForCards();
        foreach (var card in cards)
            Step($"Check image of {card.Title}"); // scrolling each card triggers lazy loading
        var images = cards.Where(c => c.HasImage).Select(c => (c.Title, Image: c.ImageElement!)).ToList();
        WaitHelper.TryUntil(Session.Driver, d => images.All(i => JavaScriptHelper.Execute(d, "return arguments[0].complete;", i.Image) is true), Session.LongTimeout);
        var broken = images.Where(i => !JavaScriptHelper.IsImageLoaded(Session.Driver, i.Image))
            .Select(i => $"{i.Title}: {i.Image.GetDomProperty("src")}").ToList();
        Assert.That(broken, Is.Empty, $"{broken.Count} of {images.Count} card images did not load:\n" + string.Join("\n", broken));
    }

    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    [TestCase(4)]
    public void Card_Opens_Matching_Details_Page(int position)
    {
        var page = new SearchPage(Session);
        page.Open();
        var cards = page.WaitForCards();
        if (cards.Count < position) Assert.Inconclusive($"Only {cards.Count} listings available.");
        var card = cards[position - 1];
        var title = TextParser.Normalize(card.Title);
        var href = card.Href;
        card.Open();
        Session.WaitFor("the details page", d => UrlHelper.SamePath(d.Url, href), Session.LongTimeout);
        var details = new ListingDetailsPage(Session);
        Assert.That(TextParser.Normalize(details.SectionTitleText), Does.Contain(title).IgnoreCase,
            $"Clicked '{title}' but the details heading says '{details.SectionTitleText}'.");
    }
}
