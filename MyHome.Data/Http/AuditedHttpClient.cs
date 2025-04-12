using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyHome.Core.Interfaces;
using MyHome.Core.Models.Audit;
using MyHome.Core.Options;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace MyHome.Data.Http;

public class AuditedHttpClient<TOptions>(
    IHttpClientFactory httpClientFactory,
    IRepository<AuditEvent> auditRepository,
    IOptions<ExternalClientOptions<TOptions>> options,
    ILogger<AuditedHttpClient<TOptions>> logger) where TOptions : ExternalClientOptions<TOptions>
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly IRepository<AuditEvent> _auditRepository = auditRepository;
    private readonly ILogger<AuditedHttpClient<TOptions>> _logger = logger;
    private readonly ExternalClientOptions<TOptions> _options = options?.Value ?? throw new ArgumentNullException(nameof(options));

    public async Task<T> GetAsync<T>(string requestUri, CancellationToken cancellationToken) where T : class
    {
        var httpClient = _httpClientFactory.CreateClient(_options.Name);

        return await httpClient.GetFromJsonAsync<T>(requestUri, cancellationToken)
            ?? throw new HttpRequestException($"Get from external client failed. URL: {httpClient.BaseAddress}. URI: {requestUri}");
    }

    public async Task<T?> PostAsync<T>(object body, string requestUri, AuditEvent auditEvent, CancellationToken cancellationToken) where T : class
    {
        var response = await PostAsync(body, requestUri, cancellationToken);

        var responseContent = await GetResponseContent<T>(response);

        await SaveAuditEventAsync(auditEvent, response, cancellationToken);

        return responseContent;
    }

    public async Task<HttpStatusCode> PatchAsync(Dictionary<string, object> body, string requestUri, AuditEvent auditEvent, CancellationToken cancellationToken)
    {
        var httpClient = _httpClientFactory.CreateClient(_options.Name);

        var response = await httpClient.PatchAsJsonAsync(requestUri, body, cancellationToken);

        await SaveAuditEventAsync(auditEvent, response, cancellationToken);

        return response.StatusCode;
    }

    private async Task<HttpResponseMessage> PostAsync(object body, string requestUri, CancellationToken cancellationToken)
    {
        var stringBody = JsonSerializer.Serialize(body);
        var requestContent = new StringContent(stringBody);

        var httpClient = _httpClientFactory.CreateClient(_options.Name);
        return await httpClient.PostAsync(requestUri, requestContent, cancellationToken);
    }

    private async Task SaveAuditEventAsync(AuditEvent auditEvent, HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            await _auditRepository.UpsertAsync(auditEvent);
        }
    }

    private async Task<T?> GetResponseContent<T>(HttpResponseMessage response) where T : class
    {
        var contentString = await GetResponseContentAsString(response);

        return string.IsNullOrEmpty(contentString)
            ? null
            : JsonSerializer.Deserialize<T>(contentString);

    }

    private async Task<string?> GetResponseContentAsString(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("External HTTP call failed. Response code: {statuscode}. Respone message: {message}", response.StatusCode, content);

            return null;
        }

        return await response.Content.ReadAsStringAsync();
    }
}
