using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MyHome.Core.Options;
using System.Net.Http.Json;
using System.Text.Json;

namespace MyHome.Data.Http;

public class OAuthHandler(
    IHttpClientFactory httpClientFactory,
    IMemoryCache cache,
    IOptions<HeatPumpClientOptions> options) : DelegatingHandler
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly IMemoryCache _cache = cache;
    private readonly HeatPumpClientOptions _options = options.Value;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await _cache.GetOrCreateAsync($"token-{_options.ClientIdentifier}", async entry =>
        {
            var token = await GetAuthToken();
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(token.ExpiresIn - 100);

            return token;
        });
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token?.AccessToken);

        var response = await base.SendAsync(request, cancellationToken);

        return response;
    }

    private async Task<Token> GetAuthToken()
    {
        var requestBody = GetRequestBody();

        var authResponse = await RequestToken(requestBody);

        return await GetReponseBody(authResponse);
    }

    private async Task<HttpResponseMessage> RequestToken(FormUrlEncodedContent requestBody)
    {
        var httpClient = _httpClientFactory.CreateClient();
        httpClient.BaseAddress = _options.BaseAddress;
        var authResponse = await httpClient.PostAsync("oauth/token", requestBody);
        return authResponse;
    }

    private static async Task<Token> GetReponseBody(HttpResponseMessage authResponse)
    {
        if (!authResponse.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Auth failed with status {authResponse.StatusCode}");
        }

        var authResponseContentString = await authResponse.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<Token>(authResponseContentString)
            ?? throw new HttpRequestException("Deserialize reponse error: " + authResponseContentString);
    }

    private FormUrlEncodedContent GetRequestBody()
    {
        if (_options.ClientIdentifier is null || _options.ClientSecret is null)
        {
            throw new ArgumentException("Cannot get Auth token. ClientIdentifier or ClientSecret is null");
        }

        var postData = new Dictionary<string, string>
        {
            { "client_id", _options.ClientIdentifier },
            { "client_secret", _options.ClientSecret },
            { "grant_type", "client_credentials" }
        };

        var content = new FormUrlEncodedContent(postData);
        return content;
    }
}

public class EbecoOptions
{
    public required string BaseAddress { get; init; }
    public required string Username { get; init; }
    public required string Password { get; init; }
}

public class EbecoToken
{
    public string? AccessToken { get; set; }
    public int ExpiresIn { get; set; }
    public long UserId { get; set; }
}

public class EbecoAuthHandler(
    IHttpClientFactory httpClientFactory,
    IMemoryCache cache,
    IOptions<EbecoOptions> options) : DelegatingHandler
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly IMemoryCache _cache = cache;
    private readonly EbecoOptions _options = options.Value;
    private const int TenantId = 1;

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var token = await _cache.GetOrCreateAsync($"token-ebeco-{_options.Username}", async entry =>
        {
            var token = await GetAuthToken(cancellationToken);
            // Cache token for slightly less than its expiration time to avoid edge cases
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(token.ExpiresIn - 100);
            return token;
        });

        if (token?.AccessToken == null)
        {
            throw new HttpRequestException("Failed to obtain access token");
        }

        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
            "Bearer",
            token.AccessToken);

        var response = await base.SendAsync(request, cancellationToken);
        return response;
    }

    private async Task<EbecoToken> GetAuthToken(CancellationToken cancellationToken)
    {
        var requestBody = GetRequestBody();
        var authResponse = await RequestToken(requestBody, cancellationToken);
        return await GetResponseBody(authResponse, cancellationToken);
    }

    private async Task<HttpResponseMessage> RequestToken(
        object requestBody,
        CancellationToken cancellationToken)
    {
        var httpClient = _httpClientFactory.CreateClient();
        httpClient.BaseAddress = new Uri(_options.BaseAddress);

        // Ebeco requires the Abp.TenantId header
        httpClient.DefaultRequestHeaders.Add("Abp.TenantId", TenantId.ToString());

        var authResponse = await httpClient.PostAsJsonAsync(
            "api/TokenAuth",
            requestBody,
            cancellationToken);

        return authResponse;
    }

    private static async Task<EbecoToken> GetResponseBody(
        HttpResponseMessage authResponse,
        CancellationToken cancellationToken)
    {
        if (!authResponse.IsSuccessStatusCode)
        {
            var errorContent = await authResponse.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException(
                $"Ebeco auth failed with status {authResponse.StatusCode}. Response: {errorContent}");
        }

        var token = await authResponse.Content.ReadFromJsonAsync<EbecoToken>(
            cancellationToken: cancellationToken);

        return token ?? throw new HttpRequestException("Failed to deserialize Ebeco token response");
    }

    private object GetRequestBody()
    {
        if (string.IsNullOrEmpty(_options.Username) || string.IsNullOrEmpty(_options.Password))
        {
            throw new ArgumentException(
                "Cannot get Ebeco auth token. Username or Password is null or empty");
        }

        return new
        {
            userNameOrEmailAddress = _options.Username,
            password = _options.Password
        };
    }
}