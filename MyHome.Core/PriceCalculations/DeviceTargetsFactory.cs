using MyHome.Core.Models.Entities.Profiles;
using MyHome.Core.Models.Interfaces;
using MyHome.Core.Models.PriceCalculations;
using EnergyPriceLevel = MyHome.Core.Models.EnergySupplier.Enums.EnergyPriceLevel;

namespace MyHome.Core.PriceCalculations;

public class DeviceTargetsFactory
{
    public static DeviceTargets CreateFromLevel(EnergyPriceLevel priceLevel, DeviceSettingsProfile profile)
    {
        var delta = priceLevel switch
        {
            EnergyPriceLevel.VeryCheap => 2,
            EnergyPriceLevel.Cheap => 1,
            EnergyPriceLevel.Expensive => -2,
            EnergyPriceLevel.VeryExpensive => -4,
            _ => 0
        };

        return new DeviceTargets
        {
            HeatOffset = profile.HeatOffsetBaseline + delta,
            ComfortMode = GetMode(priceLevel, profile.ComfortModes),
            OpMode = GetMode(priceLevel, profile.OpModes),
            ThermostatBathZeroTemperature = profile.ThermostatBathZeroTemperatureBaseline + delta,
            ThermostatBathOneTemperature = profile.ThermostatBathOneTemperatureBaseline + delta,
            ThermostatGarageTemperature = profile.ThermostatGarageTemperatureBaseline + delta,
            DehumidifierTargetHumidity = profile.DehumidifierTargetHumidityBaseline - delta
        };
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
