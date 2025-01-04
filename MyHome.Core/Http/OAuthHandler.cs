using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MyHome.Core.Models;
using MyHome.Core.Options;
using System.Net.Http;
using System.Text.Json;

namespace MyHome.Core.Http;

public class OAuthHandler<TOptions>(
    IHttpClientFactory httpClientFactory,
    IMemoryCache cache,
    IOptions<ExternalClientOptions<TOptions>> options) : DelegatingHandler where TOptions : ExternalClientOptions<TOptions>
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly IMemoryCache _cache = cache;
    private readonly ExternalClientOptions<TOptions> _options = options.Value;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await _cache.GetOrCreateAsync($"token-{_options.ClientIdentifier}", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(12);
            return await GetAuthToken();
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
        httpClient.BaseAddress = new Uri(_options.BaseAddress);
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