using Carabia.Automation.Framework.Base;
using OpenQA.Selenium;

namespace Carabia.Automation.Framework.Components.Sell;

/// <summary>Step 4 asking price: #ps-price-value, market value hints (#sellCarMarketValueRange, #sellCarPriceTagMessage), next [data-step-next='5'].</summary>
public sealed class PriceSelectorComponent(BrowserSession session) : ComponentBase(session)
{
    private const string PriceInput = "ps-price-value";
    private static readonly By MarketValueRange = By.Id("sellCarMarketValueRange");
    private static readonly By PriceTagMessage = By.Id("sellCarPriceTagMessage");
    private static readonly By Next = By.CssSelector("#sell-car-step-4 [data-step-next='5']");

    private SellWizardComponent Wizard { get; } = new(session);

    public void EnterPrice(string price) => Wizard.Fill(PriceInput, price);

    public string MarketValueText => Driver.FindElements(MarketValueRange).FirstOrDefault()?.Text.Trim() ?? string.Empty;
    public string PriceTagText => Driver.FindElements(PriceTagMessage).FirstOrDefault()?.Text.Trim() ?? string.Empty;

    public bool NextEnabled => Driver.FindElement(Next).Enabled;

    public void Continue()
    {
        Wizard.Click(Driver.FindElement(Next), "Sell: continue to photos");
        Wizard.WaitForStep(5);
    }
}
