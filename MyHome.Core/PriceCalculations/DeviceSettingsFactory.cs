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
                profile.ThermostatBathZeroTemperatures.Baseline),
            DeviceSettingsMode.Enhanced => new DeviceSettings(
                profile.HeatOffsets.Enhanced,
                profile.RadiatorTemperatures.Enhanced,
                profile.ComfortModes.Enhanced,
                profile.OpModes.Enhanced,
                profile.ThermostatBathZeroTemperatures.Enhanced),
            DeviceSettingsMode.Moderate => new DeviceSettings(
                profile.HeatOffsets.Moderate,
                profile.RadiatorTemperatures.Moderate,
                profile.ComfortModes.Moderate,
                profile.OpModes.Moderate,
                profile.ThermostatBathZeroTemperatures.Moderate),
            DeviceSettingsMode.Economic => new DeviceSettings(
                profile.HeatOffsets.Economic,
                profile.RadiatorTemperatures.Economic,
                profile.ComfortModes.Economic,
                profile.OpModes.Economic,
                profile.ThermostatBathZeroTemperatures.Economic),
            DeviceSettingsMode.MaxSavings => new DeviceSettings(
                profile.HeatOffsets.MaxSavings,
                profile.RadiatorTemperatures.MaxSavings,
                profile.ComfortModes.MaxSavings,
                profile.OpModes.MaxSavings,
                profile.ThermostatBathZeroTemperatures.MaxSavings),
            DeviceSettingsMode.ExtremeSavings => new DeviceSettings(
                profile.HeatOffsets.ExtremeSavings,
                profile.RadiatorTemperatures.ExtremeSavings,
                profile.ComfortModes.ExtremeSavings,
                profile.OpModes.ExtremeSavings,
                profile.ThermostatBathZeroTemperatures.ExtremeSavings),
            _ => new DeviceSettings(
                profile.HeatOffsets.Baseline,
                profile.RadiatorTemperatures.Baseline,
                profile.ComfortModes.Baseline,
                profile.OpModes.Baseline,
                profile.ThermostatBathZeroTemperatures.Baseline)
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
                profile.ThermostatBathZeroTemperatures.Baseline),
            EnergyPriceLevel.VeryCheap => new DeviceSettings(
                profile.HeatOffsets.Enhanced,
                profile.RadiatorTemperatures.Enhanced,
                profile.ComfortModes.Enhanced,
                profile.OpModes.Enhanced,
                profile.ThermostatBathZeroTemperatures.Enhanced),
            EnergyPriceLevel.Cheap => new DeviceSettings(
                profile.HeatOffsets.Moderate,
                profile.RadiatorTemperatures.Moderate,
                profile.ComfortModes.Moderate,
                profile.OpModes.Moderate,
                profile.ThermostatBathZeroTemperatures.Moderate),
            EnergyPriceLevel.Expensive => new DeviceSettings(
                profile.HeatOffsets.Economic,
                profile.RadiatorTemperatures.Economic,
                profile.ComfortModes.Economic,
                profile.OpModes.Economic,
                profile.ThermostatBathZeroTemperatures.Economic),
            EnergyPriceLevel.VeryExpensive => new DeviceSettings(
                profile.HeatOffsets.MaxSavings,
                profile.RadiatorTemperatures.MaxSavings,
                profile.ComfortModes.MaxSavings,
                profile.OpModes.MaxSavings,
                profile.ThermostatBathZeroTemperatures.MaxSavings),
            EnergyPriceLevel.Extreme => new DeviceSettings(
                profile.HeatOffsets.ExtremeSavings,
                profile.RadiatorTemperatures.ExtremeSavings,
                profile.ComfortModes.ExtremeSavings,
                profile.OpModes.ExtremeSavings,
                profile.ThermostatBathZeroTemperatures.ExtremeSavings),
            _ => new DeviceSettings(
                profile.HeatOffsets.Baseline,
                profile.RadiatorTemperatures.Baseline,
                profile.ComfortModes.Baseline,
                profile.OpModes.Baseline,
                profile.ThermostatBathZeroTemperatures.Baseline)
        };
    }
}
