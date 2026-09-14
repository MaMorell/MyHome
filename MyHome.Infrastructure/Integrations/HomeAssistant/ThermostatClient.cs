using MyHome.Core.Interfaces;

namespace MyHome.Infrastructure.Integrations.HomeAssistant;

/// <summary>
/// Client for controlling a climate entity via Home Assistant.
/// </summary>
public class ThermostatClient : IThermostatClient
{
    private const string TemperatureAttribute = "temperature";

    private readonly IHomeAssistantClient _homeAssistantClient;
    private readonly string _entityId;

    public ThermostatClient(IHomeAssistantClient homeAssistantClient, string entityId)
    {
        _homeAssistantClient = homeAssistantClient ?? throw new ArgumentNullException(nameof(homeAssistantClient));
        _entityId = entityId ?? throw new ArgumentNullException(nameof(entityId));
    }

    public async Task<double> GetSetTemperatureAsync()
    {
        var state = await _homeAssistantClient.GetStateAsync(_entityId);

        if (!state.Attributes.TryGetValue(TemperatureAttribute, out var temperatureElement))
        {
            throw new InvalidOperationException($"Attribute '{TemperatureAttribute}' not found for entity '{_entityId}'");
        }

        return temperatureElement.GetDouble();
    }

    public async Task UpdateSetTemperatureAsync(int temperature)
    {
        await _homeAssistantClient.CallServiceAsync(
            "climate",
            "set_temperature",
            new { entity_id = _entityId, temperature },
            CancellationToken.None);
    }
}
