using MyHome.Core.Models.Entities.Profiles;
using EnergyPriceLevel = MyHome.Core.Models.EnergySupplier.Enums.EnergyPriceLevel;

namespace MyHome.Core.PriceCalculations;

public class DeviceSettingsFactory
{
    public static DeviceSettings CreateFromLevel(EnergyPriceLevel priceLevel, DeviceSettingsProfile profile)
    {
        return priceLevel switch
        {
            EnergyPriceLevel.Normal => new DeviceSettings(
                profile.HeatOffsets.Baseline,
                profile.ComfortModes.Baseline,
                profile.OpModes.Baseline,
                profile.ThermostatBathZeroTemperatures.Baseline,
                profile.ThermostatBathOneTemperatures.Baseline,
                profile.ThermostatGarageTemperatures.Baseline,
                profile.DehumidifierTargetHumidities.Baseline),
            EnergyPriceLevel.VeryCheap => new DeviceSettings(
                profile.HeatOffsets.Enhanced,
                profile.ComfortModes.Enhanced,
                profile.OpModes.Enhanced,
                profile.ThermostatBathZeroTemperatures.Enhanced,
                profile.ThermostatBathOneTemperatures.Enhanced,
                profile.ThermostatGarageTemperatures.Enhanced,
                profile.DehumidifierTargetHumidities.Enhanced),
            EnergyPriceLevel.Cheap => new DeviceSettings(
                profile.HeatOffsets.Moderate,
                profile.ComfortModes.Moderate,
                profile.OpModes.Moderate,
                profile.ThermostatBathZeroTemperatures.Moderate,
                profile.ThermostatBathOneTemperatures.Moderate,
                profile.ThermostatGarageTemperatures.Moderate,
                profile.DehumidifierTargetHumidities.Moderate),
            EnergyPriceLevel.Expensive => new DeviceSettings(
                profile.HeatOffsets.Economic,
                profile.ComfortModes.Economic,
                profile.OpModes.Economic,
                profile.ThermostatBathZeroTemperatures.Economic,
                profile.ThermostatBathOneTemperatures.Economic,
                profile.ThermostatGarageTemperatures.Economic,
                profile.DehumidifierTargetHumidities.Economic),
            EnergyPriceLevel.VeryExpensive => new DeviceSettings(
                profile.HeatOffsets.MaxSavings,
                profile.ComfortModes.MaxSavings,
                profile.OpModes.MaxSavings,
                profile.ThermostatBathZeroTemperatures.MaxSavings,
                profile.ThermostatBathOneTemperatures.MaxSavings,
                profile.ThermostatGarageTemperatures.MaxSavings,
                profile.DehumidifierTargetHumidities.MaxSavings),
            _ => new DeviceSettings(
                profile.HeatOffsets.Baseline,
                profile.ComfortModes.Baseline,
                profile.OpModes.Baseline,
                profile.ThermostatBathZeroTemperatures.Baseline,
                profile.ThermostatBathOneTemperatures.Baseline,
                profile.ThermostatGarageTemperatures.Baseline,
                profile.DehumidifierTargetHumidities.Baseline)
        };
    }
}
