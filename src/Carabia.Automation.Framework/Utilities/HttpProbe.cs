using System.Net;

namespace Carabia.Automation.Framework.Utilities;

/// <summary>
/// Plain HTTP checks (status codes, redirects, sitemap XML). A browser renders error pages normally,
/// so HTTP status is checked separately, as in carabia-ui-tests.
/// </summary>
public sealed class HttpProbe : IDisposable
{
    private readonly HttpClient _following;
    private readonly HttpClient _notFollowing;
    private readonly Uri _baseUri;

    public HttpProbe(string baseUrl, int timeoutSeconds = 30)
    {
        _baseUri = new Uri(baseUrl.TrimEnd('/') + "/");
        _following = new HttpClient(new HttpClientHandler { AllowAutoRedirect = true, ServerCertificateCustomValidationCallback = (_, _, _, _) => true })
        { Timeout = TimeSpan.FromSeconds(timeoutSeconds) };
        _notFollowing = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false, ServerCertificateCustomValidationCallback = (_, _, _, _) => true })
        { Timeout = TimeSpan.FromSeconds(timeoutSeconds) };
        foreach (var client in new[] { _following, _notFollowing })
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Carabia.Automation/1.0 (+Selenium NUnit)");
    }

    public Uri Absolute(string pathOrUrl) =>
        UrlHelper.TryHttpUri(pathOrUrl, out var absolute) ? absolute : new Uri(_baseUri, pathOrUrl.TrimStart('/'));

    public async Task<HttpCheck> GetAsync(string pathOrUrl, bool followRedirects = true)
    {
        var uri = Absolute(pathOrUrl);
        try
        {
            using var response = await (followRedirects ? _following : _notFollowing).GetAsync(uri);
            var body = await response.Content.ReadAsStringAsync();
            return new HttpCheck(uri, response.StatusCode, response.RequestMessage?.RequestUri ?? uri,
                response.Headers.Location, response.Content.Headers.ContentType?.MediaType, body, null);
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
        {
            return new HttpCheck(uri, 0, uri, null, null, string.Empty, exception.Message);
        }
    }

    public async Task<HttpCheck> PostFormAsync(string path, IDictionary<string, string> form)
    {
        var uri = Absolute(path);
        try
        {
            using var response = await _following.PostAsync(uri, new FormUrlEncodedContent(form));
            var body = await response.Content.ReadAsStringAsync();
            return new HttpCheck(uri, response.StatusCode, response.RequestMessage?.RequestUri ?? uri, null,
                response.Content.Headers.ContentType?.MediaType, body, null);
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
        {
            return new HttpCheck(uri, 0, uri, null, null, string.Empty, exception.Message);
        }
    }

    public void Dispose()
    {
        _following.Dispose();
        _notFollowing.Dispose();
    }
}

public sealed record HttpCheck(Uri Requested, HttpStatusCode Status, Uri FinalUri, Uri? Location, string? ContentType, string Body, string? Error)
{
    public int Code => (int)Status;
    public bool IsSuccess => Error is null && Code is >= 200 and < 300;
    public bool IsRedirect => Code is 301 or 302 or 303 or 307 or 308;

    public override string ToString() => Error is not null
        ? $"{Requested} -> request failed: {Error}"
        : $"{Requested} -> HTTP {Code}{(Location is null ? "" : $" Location: {Location}")}{(FinalUri != Requested ? $" final: {FinalUri}" : "")}";
}
