using System.Globalization;
using MyHome.Core.Interfaces;

namespace MyHome.Infrastructure.Integrations.HomeAssistant;

/// <summary>
/// Reads the Tibber Pulse accumulated consumption for the current hour
/// through Home Assistant.
/// </summary>
public sealed class HomeAssistantEnergyClient(IHomeAssistantClient homeAssistantClient)
    : IHomeAssistantEnergyClient
{
    public const string AccumulatedConsumptionCurrentHourEntityId =
        "sensor.tibber_pulse_hemmet_accumulated_consumption_current_hour";

    public async Task<decimal> GetAccumulatedConsumptionCurrentHourAsync(
        CancellationToken cancellationToken = default)
    {
        var state = await homeAssistantClient.GetStateAsync(
            AccumulatedConsumptionCurrentHourEntityId,
            cancellationToken);

        if (state.State is "unknown" or "unavailable")
        {
            throw new InvalidOperationException(
                $"Home Assistant entity '{AccumulatedConsumptionCurrentHourEntityId}' is {state.State}.");
        }

        if (!decimal.TryParse(
                state.State,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out var consumption))
        {
            throw new FormatException(
                $"Home Assistant entity '{AccumulatedConsumptionCurrentHourEntityId}' returned an invalid " +
                $"consumption value: '{state.State}'.");
        }

        return consumption;
    }
}
