using MyHome.Core.Interfaces;

namespace MyHome.Infrastructure.Integrations.HomeAssistant;

public class DehumidifierClient(IHomeAssistantClient homeAssistantClient) : IDehumidifierClient
{
    private const string PowerEntityId = "switch.luftavfuktare_50l";
    private const string HumidifierEntityId = "humidifier.luftavfuktare_50l";

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
}
