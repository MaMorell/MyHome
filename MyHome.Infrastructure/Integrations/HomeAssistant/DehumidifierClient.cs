using MyHome.Core.Interfaces;
using MyHome.Core.Models.Integrations.HomeAssistant;
using System.Text.Json;

namespace MyHome.Infrastructure.Integrations.HomeAssistant;

public class DehumidifierClient(IHomeAssistantClient homeAssistantClient) : IDehumidifierClient
{
    private const string PowerEntityId = "switch.luftavfuktare_50l";
    private const string HumidifierEntityId = "humidifier.luftavfuktare_50l";

    public async Task<DehumidifierStatus> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        var powerState = await homeAssistantClient.GetStateAsync(PowerEntityId, cancellationToken);
        var humidifierState = await homeAssistantClient.GetStateAsync(HumidifierEntityId, cancellationToken);

        return new DehumidifierStatus
        {
            IsOn = powerState.State.Equals("on", StringComparison.OrdinalIgnoreCase),
            Mode = GetString(humidifierState.Attributes, "mode") ?? throw new InvalidOperationException("Mode attribute is missing"),
            TargetHumidityPercent = GetInt32(humidifierState.Attributes, "humidity") ?? throw new InvalidOperationException("Humidity attribute is missing"),
            CurrentHumidityPercent = GetInt32(humidifierState.Attributes, "current_humidity")
        };
    }

    public async Task SetPowerAsync(bool isOn, CancellationToken cancellationToken = default)
    {
        var service = isOn ? "turn_on" : "turn_off";
        await homeAssistantClient.CallServiceAsync("switch", service, new { entity_id = PowerEntityId }, cancellationToken);
    }

    public async Task SetTargetHumidityAsync(int targetHumidityPercent, CancellationToken cancellationToken = default)
    {
        await homeAssistantClient.CallServiceAsync(
            "humidifier",
            "set_humidity",
            new { entity_id = HumidifierEntityId, humidity = targetHumidityPercent },
            cancellationToken);
    }

    public async Task SetModeAsync(string mode, CancellationToken cancellationToken = default)
    {
        await homeAssistantClient.CallServiceAsync(
            "humidifier",
            "set_mode",
            new { entity_id = HumidifierEntityId, mode },
            cancellationToken);
    }

    private static string? GetString(Dictionary<string, JsonElement> attributes, string key) =>
        attributes.TryGetValue(key, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;

    private static int? GetInt32(Dictionary<string, JsonElement> attributes, string key) =>
        attributes.TryGetValue(key, out var value) && value.ValueKind == JsonValueKind.Number
            ? value.GetInt32()
            : null;
}
