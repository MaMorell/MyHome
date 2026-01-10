using MyHome.Core.Models.Entities.Profiles;
using EnergyPriceLevel = MyHome.Core.Models.EnergySupplier.Enums.EnergyPriceLevel;

namespace MyHome.Core.PriceCalculations;

public class DeviceSettingsFactory
{
    public static DeviceSettings CreateFromMode(DeviceSettingsMode mode, DeviceSettingsProfile profile)
    {
        return mode switch
        {
            DeviceSettingsMode.Baseline => new DeviceSettings(
                profile.HeatOffsets.Baseline,
                profile.RadiatorTemperatures.Baseline,
                profile.ComfortModes.Baseline,
                profile.OpModes.Baseline,
                profile.ThermostatTuyaTemperatures.Baseline),
            DeviceSettingsMode.Enhanced => new DeviceSettings(
                profile.HeatOffsets.Enhanced,
                profile.RadiatorTemperatures.Enhanced,
                profile.ComfortModes.Enhanced,
                profile.OpModes.Enhanced,
                profile.ThermostatTuyaTemperatures.Enhanced),
            DeviceSettingsMode.Moderate => new DeviceSettings(
                profile.HeatOffsets.Moderate,
                profile.RadiatorTemperatures.Moderate,
                profile.ComfortModes.Moderate,
                profile.OpModes.Moderate,
                profile.ThermostatTuyaTemperatures.Moderate),
            DeviceSettingsMode.Economic => new DeviceSettings(
                profile.HeatOffsets.Economic,
                profile.RadiatorTemperatures.Economic,
                profile.ComfortModes.Economic,
                profile.OpModes.Economic,
                profile.ThermostatTuyaTemperatures.Economic),
            DeviceSettingsMode.MaxSavings => new DeviceSettings(
                profile.HeatOffsets.MaxSavings,
                profile.RadiatorTemperatures.MaxSavings,
                profile.ComfortModes.MaxSavings,
                profile.OpModes.MaxSavings,
                profile.ThermostatTuyaTemperatures.MaxSavings),
            DeviceSettingsMode.ExtremeSavings => new DeviceSettings(
                profile.HeatOffsets.ExtremeSavings,
                profile.RadiatorTemperatures.ExtremeSavings,
                profile.ComfortModes.ExtremeSavings,
                profile.OpModes.ExtremeSavings,
                profile.ThermostatTuyaTemperatures.ExtremeSavings),
            _ => new DeviceSettings(
                profile.HeatOffsets.Baseline,
                profile.RadiatorTemperatures.Baseline,
                profile.ComfortModes.Baseline,
                profile.OpModes.Baseline,
                profile.ThermostatTuyaTemperatures.Baseline)
        };
    }

    public static DeviceSettings CreateFromLevel(EnergyPriceLevel priceLevel, DeviceSettingsProfile profile)
    {
        return priceLevel switch
        {
            EnergyPriceLevel.Normal => new DeviceSettings(
                profile.HeatOffsets.Baseline,
                profile.RadiatorTemperatures.Baseline,
                profile.ComfortModes.Baseline,
                profile.OpModes.Baseline,
                profile.ThermostatTuyaTemperatures.Baseline),
            EnergyPriceLevel.VeryCheap => new DeviceSettings(
                profile.HeatOffsets.Enhanced,
                profile.RadiatorTemperatures.Enhanced,
                profile.ComfortModes.Enhanced,
                profile.OpModes.Enhanced,
                profile.ThermostatTuyaTemperatures.Enhanced),
            EnergyPriceLevel.Cheap => new DeviceSettings(
                profile.HeatOffsets.Moderate,
                profile.RadiatorTemperatures.Moderate,
                profile.ComfortModes.Moderate,
                profile.OpModes.Moderate,
                profile.ThermostatTuyaTemperatures.Moderate),
            EnergyPriceLevel.Expensive => new DeviceSettings(
                profile.HeatOffsets.Economic,
                profile.RadiatorTemperatures.Economic,
                profile.ComfortModes.Economic,
                profile.OpModes.Economic,
                profile.ThermostatTuyaTemperatures.Economic),
            EnergyPriceLevel.VeryExpensive => new DeviceSettings(
                profile.HeatOffsets.MaxSavings,
                profile.RadiatorTemperatures.MaxSavings,
                profile.ComfortModes.MaxSavings,
                profile.OpModes.MaxSavings,
                profile.ThermostatTuyaTemperatures.MaxSavings),
            EnergyPriceLevel.Extreme => new DeviceSettings(
                profile.HeatOffsets.ExtremeSavings,
                profile.RadiatorTemperatures.ExtremeSavings,
                profile.ComfortModes.ExtremeSavings,
                profile.OpModes.ExtremeSavings,
                profile.ThermostatTuyaTemperatures.ExtremeSavings),
            _ => new DeviceSettings(
                profile.HeatOffsets.Baseline,
                profile.RadiatorTemperatures.Baseline,
                profile.ComfortModes.Baseline,
                profile.OpModes.Baseline,
                profile.ThermostatTuyaTemperatures.Baseline)
        };
    }
}
