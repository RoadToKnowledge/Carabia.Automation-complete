using Carabia.Automation.Framework.Configuration;

namespace Carabia.Automation.Framework.Base;

/// <summary>A business flow that spans several pages (browse -> listing -> offer ...).</summary>
public abstract class FlowBase(BrowserSession session)
{
    protected BrowserSession Session { get; } = session;
    protected TestSettings Settings => Session.Settings;
    protected void Step(string message) => Session.Step(message);
}
