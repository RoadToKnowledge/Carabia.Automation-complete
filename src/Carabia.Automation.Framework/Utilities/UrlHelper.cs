namespace Carabia.Automation.Framework.Utilities;

public static class UrlHelper
{
    public static Uri Combine(string baseUrl, string relative) =>
        new(new Uri(baseUrl.TrimEnd('/') + "/"), relative.TrimStart('/'));

    /// <summary>
    /// Absolute http(s) URL only. On Linux, Uri.TryCreate("/path", Absolute) yields a file:// URI, so a plain
    /// TryCreate is not enough to tell a route from a URL.
    /// </summary>
    public static bool TryHttpUri(string? value, out Uri uri)
    {
        if (Uri.TryCreate(value, UriKind.Absolute, out var parsed) && (parsed.Scheme == Uri.UriSchemeHttp || parsed.Scheme == Uri.UriSchemeHttps))
        {
            uri = parsed;
            return true;
        }
        uri = null!;
        return false;
    }

    /// <summary>Path + query of a URL, or the value itself when it already is a route.</summary>
    public static string RouteOf(string pathOrUrl) => TryHttpUri(pathOrUrl, out var uri) ? uri.PathAndQuery : pathOrUrl;

    public static string PathOf(string url) =>
        TryHttpUri(url, out var uri) ? uri.AbsolutePath.TrimEnd('/') is { Length: > 0 } p ? p : "/" : url.Split('?')[0];

    public static bool SamePath(string a, string b) =>
        string.Equals(PathOf(a), PathOf(b), StringComparison.OrdinalIgnoreCase);

    public static bool IsSameHost(string url, string baseUrl) =>
        TryHttpUri(url, out var a)
        && TryHttpUri(baseUrl, out var b)
        && a.Host.Equals(b.Host, StringComparison.OrdinalIgnoreCase);

    /// <summary>Carabia's slug rule (General/SellingCollections.Slug): lower case, non-alphanumerics collapse to '-'.</summary>
    public static string Slug(string value) =>
        System.Text.RegularExpressions.Regex.Replace((value ?? string.Empty).ToLowerInvariant(), "[^a-z0-9]+", "-").Trim('-');

    /// <summary>Listing ids come from routes like /sellings/1056_audi-a6.</summary>
    public static int? SellingIdFromUrl(string url)
    {
        var match = System.Text.RegularExpressions.Regex.Match(PathOf(url), @"/sellings/(\d+)_");
        return match.Success ? int.Parse(match.Groups[1].Value) : null;
    }
}
