using MyHome.Core.Models.Entities.Profiles;
using MyHome.Core.Models.Interfaces;
using EnergyPriceLevel = MyHome.Core.Models.EnergySupplier.Enums.EnergyPriceLevel;

namespace MyHome.Core.PriceCalculations;

public class DeviceSettingsFactory
{
    public static DeviceSettings CreateFromLevel(EnergyPriceLevel priceLevel, DeviceSettingsProfile profile)
    {
        var delta = priceLevel switch
        {
            EnergyPriceLevel.VeryCheap => 2,
            EnergyPriceLevel.Cheap => 1,
            EnergyPriceLevel.Expensive => -2,
            EnergyPriceLevel.VeryExpensive => -4,
            _ => 0
        };

        return new DeviceSettings(
            profile.HeatOffsetBaseline + delta,
            GetMode(priceLevel, profile.ComfortModes),
            GetMode(priceLevel, profile.OpModes),
            profile.ThermostatBathZeroTemperatureBaseline + delta,
            profile.ThermostatBathOneTemperatureBaseline + delta,
            profile.ThermostatGarageTemperatureBaseline + delta,
            profile.DehumidifierTargetHumidityBaseline - delta);
    }

    private static T GetMode<T>(EnergyPriceLevel priceLevel, IDeviceProfile<T> profile) => priceLevel switch
    {
        EnergyPriceLevel.VeryCheap => profile.Enhanced,
        EnergyPriceLevel.Cheap => profile.Moderate,
        EnergyPriceLevel.Expensive => profile.Economic,
        EnergyPriceLevel.VeryExpensive => profile.MaxSavings,
        _ => profile.Baseline
    };
}
