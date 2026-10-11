using Carabia.Automation.Framework.Base;

namespace Carabia.Automation.Framework.Pages.Public;

/// <summary>Any route, when a test only needs the shared page checks (title, canonical, 404 detection, body text).</summary>
public sealed class GenericPage(BrowserSession session) : PageBase(session);
