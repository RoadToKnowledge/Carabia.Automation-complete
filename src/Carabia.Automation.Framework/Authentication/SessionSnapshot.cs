namespace Carabia.Automation.Framework.Authentication;

/// <summary>
/// What is saved after a manual login: browser cookies and web storage for the Carabia origin.
/// No passwords, phone numbers or OTPs. Files live in sessions/&lt;role&gt;/ and are git-ignored.
/// </summary>
public sealed class SessionSnapshot
{
    public SessionRole Role { get; set; }
    public string BaseUrl { get; set; } = string.Empty;
    public DateTimeOffset CapturedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>User name reported by /Identity/AuthStatus (Carabia uses a GUID). Helps spot a wrong account.</summary>
    public string? AuthenticatedName { get; set; }

    public List<StoredCookie> Cookies { get; set; } = [];
    public Dictionary<string, string> LocalStorage { get; set; } = new(StringComparer.Ordinal);
    public Dictionary<string, string> SessionStorage { get; set; } = new(StringComparer.Ordinal);
}

public sealed class StoredCookie
{
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Domain { get; set; }
    public string Path { get; set; } = "/";
    public DateTime? Expiry { get; set; }
    public bool Secure { get; set; }
    public bool HttpOnly { get; set; }
    public string? SameSite { get; set; }
}
