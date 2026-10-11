using System.Text.Json;
using System.Text.Json.Serialization;
using Carabia.Automation.Framework.Configuration;
using OpenQA.Selenium;

namespace Carabia.Automation.Framework.Authentication;

/// <summary>Saves and restores role sessions in sessions/&lt;role&gt;/session.json (git-ignored).</summary>
public sealed class SessionManager(TestSettings settings)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public string GetSessionFilePath(SessionRole role)
    {
        var roleDirectory = Path.Combine(settings.ResolvePath(settings.Execution.SessionsDirectory), role.ToString().ToLowerInvariant());
        Directory.CreateDirectory(roleDirectory);
        return Path.Combine(roleDirectory, "session.json");
    }

    public bool TryLoad(SessionRole role, out SessionSnapshot? snapshot, out string reason)
    {
        snapshot = null;
        var filePath = GetSessionFilePath(role);
        if (!File.Exists(filePath))
        {
            reason = "no saved session";
            return false;
        }

        try
        {
            snapshot = JsonSerializer.Deserialize<SessionSnapshot>(File.ReadAllText(filePath), JsonOptions);
        }
        catch (JsonException)
        {
            reason = "saved session file is unreadable";
            return false;
        }

        if (snapshot is null)
        {
            reason = "saved session file is empty";
            return false;
        }

        if (!string.Equals(snapshot.BaseUrl.TrimEnd('/'), settings.BaseUrl, StringComparison.OrdinalIgnoreCase))
        {
            reason = $"saved session belongs to {snapshot.BaseUrl}, not {settings.BaseUrl}";
            return false;
        }

        var age = DateTimeOffset.UtcNow - snapshot.CapturedAtUtc;
        if (age > TimeSpan.FromHours(settings.Authentication.SessionMaxAgeHours))
        {
            reason = $"saved session is {age.TotalHours:0} hours old (limit {settings.Authentication.SessionMaxAgeHours})";
            return false;
        }

        if (!CookieStorage.HasUnexpiredAuthCookie(snapshot))
        {
            reason = $"saved session has no unexpired {CookieStorage.AuthCookieName} cookie";
            return false;
        }

        reason = "ok";
        return true;
    }

    public SessionSnapshot Save(SessionRole role, IWebDriver driver)
    {
        var status = AuthStatusProbe.Read(driver);
        var snapshot = new SessionSnapshot
        {
            Role = role,
            BaseUrl = settings.BaseUrl,
            CapturedAtUtc = DateTimeOffset.UtcNow,
            AuthenticatedName = status.Name,
            Cookies = CookieStorage.Capture(driver),
            LocalStorage = LocalStorageManager.ReadAll(driver),
            SessionStorage = LocalStorageManager.ReadAll(driver, "sessionStorage")
        };

        File.WriteAllText(GetSessionFilePath(role), JsonSerializer.Serialize(snapshot, JsonOptions));
        return snapshot;
    }

    /// <summary>Restores cookies and storage, then reloads so the server sees the auth cookie.</summary>
    public void Restore(IWebDriver driver, SessionSnapshot snapshot)
    {
        driver.Navigate().GoToUrl(settings.BaseUrl + "/");
        CookieStorage.Restore(driver, snapshot.Cookies);
        LocalStorageManager.Restore(driver, snapshot.LocalStorage);
        LocalStorageManager.Restore(driver, snapshot.SessionStorage, "sessionStorage");
        driver.Navigate().Refresh();
    }

    public void Delete(SessionRole role)
    {
        var path = GetSessionFilePath(role);
        if (File.Exists(path))
            File.Delete(path);
    }
}
