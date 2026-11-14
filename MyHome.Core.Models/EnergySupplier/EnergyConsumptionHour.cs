namespace MyHome.Core.Models.EnergySupplier;

/// <summary>
/// Measured energy consumption for a single hour of a day.
/// </summary>
public record EnergyConsumptionHour
{
    public required DateTimeOffset StartsAt { get; init; }
    public required decimal ConsumptionKWh { get; init; }
}
