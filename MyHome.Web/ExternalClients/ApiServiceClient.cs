using Microsoft.Extensions.Options;
using OpenTelemetry;
using System.Text.Json;

namespace MyHome.Web.ExternalClients;

public class ApiServiceClient
{
    private readonly IHttpClientFactory _factory;
    private readonly ILogger<ApiServiceClient> _logger;
    private readonly ApiServiceOptions _options;
    private readonly JsonSerializerOptions _jsonSerializerOptions = new() 
    { 
        PropertyNameCaseInsensitive = true 
    };
    
    public ApiServiceClient(IHttpClientFactory clientFactory, IOptions<ApiServiceOptions> options, ILogger<ApiServiceClient> logger)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        _factory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
        _logger = logger;
        _options = options.Value;
    }

    public async Task<T?> GetFromJsonAsync<T>(string uri, CancellationToken cancellationToken = default)
    {
        using var client = _factory.CreateClient();
        client.BaseAddress = _options.BaseUrl;

        var response = await client.GetAsync(uri);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning($"Http request was not successful. Return code: {response.StatusCode}. Uri: {uri}");

            return default;
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrEmpty(responseContent))
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(responseContent, _jsonSerializerOptions);
    }

    public async Task PutAsJsonAsync(string uri, object body, CancellationToken cancellationToken = default)
    {
        using var client = _factory.CreateClient();
        client.BaseAddress = _options.BaseUrl;

        await client.PutAsJsonAsync(uri, body, cancellationToken);
    }
}
