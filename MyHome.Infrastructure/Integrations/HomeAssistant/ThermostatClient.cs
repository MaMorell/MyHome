using MyHome.Core.Interfaces;

namespace MyHome.Infrastructure.Integrations.HomeAssistant;

/// <summary>
/// Client for controlling the "Thermostat bath0" climate entity via Home Assistant.
/// </summary>
public class ThermostatClient(IHomeAssistantClient homeAssistantClient) : IThermostatClient
{
    private const string EntityId = "climate.thermostat_bath0_thermostat";
    private const string TemperatureAttribute = "temperature";

    public async Task<double> GetSetTemperatureAsync()
    {
        var state = await homeAssistantClient.GetStateAsync(EntityId);

        if (!state.Attributes.TryGetValue(TemperatureAttribute, out var temperatureElement))
        {
            throw new InvalidOperationException($"Attribute '{TemperatureAttribute}' not found for entity '{EntityId}'");
        }

        return temperatureElement.GetDouble();
    }

    public async Task UpdateSetTemperatureAsync(int temperature)
    {
        await homeAssistantClient.CallServiceAsync(
            "climate",
            "set_temperature",
            new { entity_id = EntityId, temperature },
            CancellationToken.None);
    }
}
