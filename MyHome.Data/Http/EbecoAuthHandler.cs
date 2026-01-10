using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MyHome.Data.Options;
using System.Net.Http.Json;

namespace MyHome.Data.Http;

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
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(token.ExpireInSeconds - 100);
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

    private async Task<EbecoTokenResult> GetAuthToken(CancellationToken cancellationToken)
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
        httpClient.BaseAddress = _options.BaseAddress;

        // Ebeco requires the Abp.TenantId header
        httpClient.DefaultRequestHeaders.Add("Abp.TenantId", TenantId.ToString());

        var authResponse = await httpClient.PostAsJsonAsync(
            "api/TokenAuth",
            requestBody,
            cancellationToken);

        return authResponse;
    }

    private static async Task<EbecoTokenResult> GetResponseBody(
        HttpResponseMessage authResponse,
        CancellationToken cancellationToken)
    {
        if (!authResponse.IsSuccessStatusCode)
        {
            var errorContent = await authResponse.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException(
                $"Ebeco auth failed with status {authResponse.StatusCode}. Response: {errorContent}");
        }

        var tokenResponse = await authResponse.Content.ReadFromJsonAsync<EbecoTokenResponse>(
            new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            },
            cancellationToken: cancellationToken);

        if (tokenResponse?.Result == null || !tokenResponse.Success)
        {
            throw new HttpRequestException("Failed to deserialize Ebeco token response or authentication was not successful");
        }

        return tokenResponse.Result;
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