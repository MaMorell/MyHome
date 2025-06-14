using MyHome.Core.Exceptions;
using MyHome.Core.Extensions;
using MyHome.Core.Interfaces;
using MyHome.Core.Models.Entities.Constants;
using MyHome.Core.Models.Entities.Profiles;
using MyHome.Core.Models.Integrations.HeatPump;
using MyHome.Core.Models.PriceCalculations;
using EnergyPriceLevel = MyHome.Core.Models.EnergySupplier.Enums.EnergyPriceLevel;

namespace MyHome.Core.PriceCalculations;

public class DeviceSettingsFactory
{
    private readonly IRepository<DeviceSettingsProfile> _deviceSettingsRepository;
    private readonly IHeatPumpClient _heatPumpClient;

    public DeviceSettingsFactory(IRepository<DeviceSettingsProfile> repository, IHeatPumpClient heatPumpClient)
    {
        _deviceSettingsRepository = repository;
        _heatPumpClient = heatPumpClient;
    }

    public async Task<DeviceSettings> CreateFromPrice(EnergyPriceDetails price)
    {
        var deviceSettings = await GetDeviceSettingsProfileAsync();

        var opMode = GetOpMode(price.LevelInternal, deviceSettings.OpModes, price);
        var targetTemprature = GetRadiatorTemperature(price.LevelInternal, deviceSettings.RadiatorTemperatures);
        var comfortMode = GetComfortMode(price.LevelInternal, deviceSettings.ComfortModes);
        var floorTemperature = GetFloorHeaterTemperature(price.LevelInternal, deviceSettings.FloorHeaterTemperatures);
        var heatOffset = await GetHeatOffset(price.LevelInternal, deviceSettings.HeatOffsets);

        return new DeviceSettings(heatOffset, targetTemprature, comfortMode, opMode, floorTemperature);
    }

    public async Task<DeviceSettings> CreateFromMode(DeviceSettingsMode mode)
    {
        var calculationTemplate = await GetDeviceSettingsProfileAsync();

        return mode switch
        {
            DeviceSettingsMode.Baseline => new DeviceSettings(
                calculationTemplate.HeatOffsets.Baseline,
                calculationTemplate.RadiatorTemperatures.Baseline,
                calculationTemplate.ComfortModes.Baseline,
                calculationTemplate.OpModes.Baseline,
                calculationTemplate.FloorHeaterTemperatures.Baseline),
            DeviceSettingsMode.Enhanced => new DeviceSettings(
                calculationTemplate.HeatOffsets.Enhanced,
                calculationTemplate.RadiatorTemperatures.Enhanced,
                calculationTemplate.ComfortModes.Enhanced,
                calculationTemplate.OpModes.Enhanced,
                calculationTemplate.FloorHeaterTemperatures.Enhanced),
            DeviceSettingsMode.Moderate => new DeviceSettings(
                calculationTemplate.HeatOffsets.Moderate,
                calculationTemplate.RadiatorTemperatures.Moderate,
                calculationTemplate.ComfortModes.Moderate,
                calculationTemplate.OpModes.Moderate,
                calculationTemplate.FloorHeaterTemperatures.Moderate),
            DeviceSettingsMode.Economic => new DeviceSettings(
                calculationTemplate.HeatOffsets.Economic,
                calculationTemplate.RadiatorTemperatures.Economic,
                calculationTemplate.ComfortModes.Economic,
                calculationTemplate.OpModes.Economic,
                calculationTemplate.FloorHeaterTemperatures.Economic),
            DeviceSettingsMode.MaxSavings => new DeviceSettings(
                calculationTemplate.HeatOffsets.MaxSavings,
                calculationTemplate.RadiatorTemperatures.MaxSavings,
                calculationTemplate.ComfortModes.MaxSavings,
                calculationTemplate.OpModes.MaxSavings,
                calculationTemplate.FloorHeaterTemperatures.MaxSavings),
            DeviceSettingsMode.ExtremeSavings => new DeviceSettings(
                calculationTemplate.HeatOffsets.ExtremeSavings,
                calculationTemplate.RadiatorTemperatures.ExtremeSavings,
                calculationTemplate.ComfortModes.ExtremeSavings,
                calculationTemplate.OpModes.ExtremeSavings,
                calculationTemplate.FloorHeaterTemperatures.ExtremeSavings),
            _ => new DeviceSettings(
                calculationTemplate.HeatOffsets.Baseline,
                calculationTemplate.RadiatorTemperatures.Baseline,
                calculationTemplate.ComfortModes.Baseline,
                calculationTemplate.OpModes.Baseline,
                calculationTemplate.FloorHeaterTemperatures.Baseline)
        };
    }

    private async Task<int> GetHeatOffset(EnergyPriceLevel priceLevel, HeatOffsetProfile profile)
    {
        var exhaustAirTemp = await _heatPumpClient.GetExhaustAirTemp(CancellationToken.None);
        if (exhaustAirTemp > 23 && priceLevel != EnergyPriceLevel.Extreme)
        {
            return profile.MaxSavings;
        }

        return priceLevel switch
        {
            EnergyPriceLevel.Normal => profile.Baseline,
            EnergyPriceLevel.VeryCheap => profile.Enhanced,
            EnergyPriceLevel.Cheap => profile.Moderate,
            EnergyPriceLevel.Expensive => profile.Economic,
            EnergyPriceLevel.VeryExpensive => profile.MaxSavings,
            EnergyPriceLevel.Extreme => profile.ExtremeSavings,
            _ => profile.Baseline,
        };
    }

    private static ComfortMode GetComfortMode(EnergyPriceLevel priceLevel, ComfortModeProfile profile)
    {
        if (DateTime.Now.IsWeekdayMidDay())
        {
            return ComfortMode.Economy;
        }

        return priceLevel switch
        {
            EnergyPriceLevel.Normal => profile.Baseline,
            EnergyPriceLevel.VeryCheap => profile.Enhanced,
            EnergyPriceLevel.Cheap => profile.Moderate,
            EnergyPriceLevel.Expensive => profile.Economic,
            EnergyPriceLevel.VeryExpensive => profile.MaxSavings,
            EnergyPriceLevel.Extreme => profile.ExtremeSavings,
            _ => profile.Baseline,
        };
    }

    private static OpMode GetOpMode(EnergyPriceLevel priceLevel, OpModeProfile profile, EnergyPriceDetails energyPrice)
    {
        var maxPriceAutoMode = DateTime.Now.IsNightTime()
            ? profile.MaxPriceAutoModeNightTime
            : profile.MaxPriceAutoMode;
        var isPriceUnderAutoModeLimit = energyPrice.PriceTotal < maxPriceAutoMode;

        if (!isPriceUnderAutoModeLimit)
        {
            return OpMode.Manual;
        }

        return priceLevel switch
        {
            EnergyPriceLevel.Normal => profile.Baseline,
            EnergyPriceLevel.VeryCheap => profile.Enhanced,
            EnergyPriceLevel.Cheap => profile.Moderate,
            EnergyPriceLevel.Expensive => profile.Economic,
            EnergyPriceLevel.VeryExpensive => profile.MaxSavings,
            EnergyPriceLevel.Extreme => profile.ExtremeSavings,
            _ => profile.Baseline,
        };
    }

    private static int GetRadiatorTemperature(EnergyPriceLevel priceLevel, RadiatorTemperatureProfile radiatorTemperatures)
    {
        return priceLevel switch
        {
            EnergyPriceLevel.Normal => radiatorTemperatures.Baseline,
            EnergyPriceLevel.VeryCheap => radiatorTemperatures.Enhanced,
            EnergyPriceLevel.Cheap => radiatorTemperatures.Moderate,
            EnergyPriceLevel.Expensive => radiatorTemperatures.Economic,
            EnergyPriceLevel.VeryExpensive => radiatorTemperatures.MaxSavings,
            EnergyPriceLevel.Extreme => radiatorTemperatures.ExtremeSavings,
            _ => radiatorTemperatures.Baseline,
        };
    }

    private static int GetFloorHeaterTemperature(EnergyPriceLevel priceLevel, FloorHeaterTemperatureProfile floorHeaterTemperatures)
    {
        var result = priceLevel switch
        {
            EnergyPriceLevel.Normal => floorHeaterTemperatures.Baseline,
            EnergyPriceLevel.VeryCheap => floorHeaterTemperatures.Enhanced,
            EnergyPriceLevel.Cheap => floorHeaterTemperatures.Moderate,
            EnergyPriceLevel.Expensive => floorHeaterTemperatures.Economic,
            EnergyPriceLevel.VeryExpensive => floorHeaterTemperatures.MaxSavings,
            EnergyPriceLevel.Extreme => floorHeaterTemperatures.ExtremeSavings,
            _ => floorHeaterTemperatures.Baseline,
        };

        result = AdjustTempratureForTimeOfDay(result);

        return result;
    }

    private static int AdjustTempratureForTimeOfDay(int result)
    {
        var now = DateTime.Now;

        if (now.IsNightTime())
        {
            result -= 4;
        }

        if (now.IsWeekdayMidDay())
        {
            result -= 3;
        }

        const int MinAllowedTemperature = 5;
        if (result < MinAllowedTemperature)
        {
            result = 5;
        }

        return result;
    }

    private async Task<DeviceSettingsProfile> GetDeviceSettingsProfileAsync()
    {
        var settings = await _deviceSettingsRepository.GetByIdAsync(EntityIdConstants.DeviceSettingsId)
            ?? throw new EntityNotFoundException(EntityIdConstants.DeviceSettingsId);

        return settings;
    }
}
