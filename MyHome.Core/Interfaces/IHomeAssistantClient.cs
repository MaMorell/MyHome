using MyHome.Core.Models.Integrations.HomeAssistant;

namespace MyHome.Core.Interfaces;

/// <summary>
/// Generic client for the REST API of a Home Assistant instance.
/// Reusable across any device exposed by that instance.
/// </summary>
public interface IHomeAssistantClient
{
    Task<HomeAssistantState> GetStateAsync(string entityId, CancellationToken cancellationToken = default);
    Task CallServiceAsync(string domain, string service, object payload, CancellationToken cancellationToken = default);
}
