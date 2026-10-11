namespace Carabia.Automation.Framework.Authentication;

/// <summary>Thrown when a role session is needed but nobody completed the manual login in time (or no one can, e.g. CI/headless).</summary>
public sealed class ManualAuthenticationRequiredException(string message) : Exception(message);
