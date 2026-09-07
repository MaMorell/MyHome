using MyHome.Core.Interfaces;
using MyHome.Core.Models.Integrations.HomeAssistant;
using System.Net.Http.Json;
using System.Text.Json;

namespace MyHome.Infrastructure.Integrations.HomeAssistant;

/// <summary>
/// Generic REST client for a Home Assistant instance. Wraps the "get entity
/// state" and "call service" endpoints so device-specific clients can be built
/// on top of it without each having to deal with HTTP/JSON directly.
/// </summary>
public class HomeAssistantClient(HttpClient httpClient) : IHomeAssistantClient
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<HomeAssistantState> GetStateAsync(string entityId, CancellationToken cancellationToken = default)
    {
        var encodedEntityId = Uri.EscapeDataString(entityId);
        var response = await httpClient.GetAsync($"api/states/{encodedEntityId}", cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);

        var state = await response.Content.ReadFromJsonAsync<HomeAssistantState>(_jsonOptions, cancellationToken);
        return state ?? throw new InvalidOperationException($"Failed to deserialize state for entity '{entityId}'");
    }

    public async Task CallServiceAsync(string domain, string service, object payload, CancellationToken cancellationToken = default)
    {
        var encodedDomain = Uri.EscapeDataString(domain);
        var encodedService = Uri.EscapeDataString(service);
        var response = await httpClient.PostAsJsonAsync($"api/services/{encodedDomain}/{encodedService}", payload, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
        throw new HttpRequestException(
            $"Home Assistant request failed with status {response.StatusCode}. Response: {errorContent}");
    }
}
