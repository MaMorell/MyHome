namespace MyHome.Core.Interfaces;

/// <summary>
/// Reads energy measurements exposed by Home Assistant.
/// </summary>
public interface IHomeAssistantEnergyClient
{
    Task<decimal> GetAccumulatedConsumptionCurrentHourAsync(
        CancellationToken cancellationToken = default);
}
