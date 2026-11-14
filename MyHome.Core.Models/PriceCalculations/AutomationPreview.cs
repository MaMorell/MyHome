using MyHome.Core.Models.Integrations.HeatPump;

namespace MyHome.Core.Models.PriceCalculations;

public enum ConsumptionOverrideStatus
{
    NotApplicable,
    BelowLimit,
    Applied,
    Unavailable
}

public sealed record DeviceTargets
{
    public required int HeatOffset { get; init; }
    public required ComfortMode ComfortMode { get; init; }
    public required OpMode OpMode { get; init; }
    public required int ThermostatBathZeroTemperature { get; init; }
    public required int ThermostatBathOneTemperature { get; init; }
    public required int ThermostatGarageTemperature { get; init; }
    public required int DehumidifierTargetHumidity { get; init; }
}
