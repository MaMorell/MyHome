using MyHome.Core.Models.Integrations.HeatPump;

namespace MyHome.Core.PriceCalculations;

public class DeviceSettings(
    int heatOffset,
    ComfortMode comfortMode,
    OpMode opMode,
    int thermostatBathZero,
    int thermostatBathOne,
    int thermostatGarage,
    int dehumidifierTargetHumidity)
{
    public int HeatOffset { get; set; } = heatOffset;
    public ComfortMode ComfortMode { get; set; } = comfortMode;
    public OpMode OpMode { get; set; } = opMode;
    public int ThermostatBathZeroTemperature { get; set; } = thermostatBathZero;
    public int ThermostatBathOneTemperature { get; set; } = thermostatBathOne;
    public int ThermostatGarageTemperature { get; set; } = thermostatGarage;
    public int DehumidifierTargetHumidity { get; set; } = dehumidifierTargetHumidity;
}