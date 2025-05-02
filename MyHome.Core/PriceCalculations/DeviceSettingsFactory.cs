using MyHome.Core.Exceptions;
using MyHome.Core.Extensions;
using MyHome.Core.Interfaces;
using MyHome.Core.Models.EnergySupplier;
using MyHome.Core.Models.EnergySupplier.Enums;
using MyHome.Core.Models.Entities.Constants;
using MyHome.Core.Models.Entities.Profiles;
using MyHome.Core.Models.Integrations.HeatPump;
using EnergyPriceLevel = MyHome.Core.Models.EnergySupplier.Enums.EnergyPriceLevel;

namespace MyHome.Core.PriceCalculations;

public class DeviceSettingsFactory
{
    private readonly IRepository<DeviceSettingsProfile> _deviceSettingsRepository;

    public DeviceSettingsFactory(IRepository<DeviceSettingsProfile> repository)
    {
        _deviceSettingsRepository = repository;
    }

    public async Task<DeviceSettings> CreateFromPrice(EnergyConsumptionEntry price)
    {
        var calculationTemplate = await GetDeviceSettingsProfileAsync();

        var opMode = GetOpMode(price);
        var heatOffset = GetHeatOffset(price.RelativePriceLevel, calculationTemplate.HeatOffsets);
        var targetTemprature = GetRadiatorTemperature(price.RelativePriceLevel, calculationTemplate.RadiatorTemperatures);
        var comfortMode = GetComfortMode(price.RelativePriceLevel, calculationTemplate.ComfortModes);
        var floorTemperature = GetFloorHeaterTemperature(price.RelativePriceLevel, calculationTemplate.FloorHeaterTemperatures);

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

    private static int GetHeatOffset(RelativePriceLevel priceLevel, HeatOffsetProfile heatOffsets)
    {
        return priceLevel switch
        {
            RelativePriceLevel.Normal => heatOffsets.Baseline,
            RelativePriceLevel.VeryLow => heatOffsets.Enhanced,
            RelativePriceLevel.Low => heatOffsets.Moderate,
            RelativePriceLevel.High => heatOffsets.Economic,
            RelativePriceLevel.VeryHigh => heatOffsets.MaxSavings,
            RelativePriceLevel.Extreme => heatOffsets.ExtremeSavings,
            _ => heatOffsets.Baseline,
        };
    }

    private static ComfortMode GetComfortMode(RelativePriceLevel priceLevel, ComfortModeProfile comfortModes)
    {
        return priceLevel switch
        {
            RelativePriceLevel.Normal => comfortModes.Baseline,
            RelativePriceLevel.VeryLow => comfortModes.Enhanced,
            RelativePriceLevel.Low => comfortModes.Moderate,
            RelativePriceLevel.High => comfortModes.Economic,
            RelativePriceLevel.VeryHigh => comfortModes.MaxSavings,
            RelativePriceLevel.Extreme => comfortModes.ExtremeSavings,
            _ => comfortModes.Baseline,
        };
    }

    private static OpMode GetOpMode(EnergyConsumptionEntry energyPrice)
    {
        var priceLevelIsLow =
            energyPrice.PriceLevel == EnergyPriceLevel.Normal ||
            energyPrice.PriceLevel == EnergyPriceLevel.Cheap ||
            energyPrice.PriceLevel == EnergyPriceLevel.VeryCheap;

        var priceLimit = DateTime.Now.IsNightTime()
            ? 1.5m
            : 1.0m;
        var priceIsCheap = energyPrice.Price < priceLimit;

        return priceLevelIsLow && priceIsCheap
            ? OpMode.Auto
            : OpMode.Manual;
    }

    private static int GetRadiatorTemperature(RelativePriceLevel priceLevel, RadiatorTemperatureProfile radiatorTemperatures)
    {
        return priceLevel switch
        {
            RelativePriceLevel.Normal => radiatorTemperatures.Baseline,
            RelativePriceLevel.VeryLow => radiatorTemperatures.Enhanced,
            RelativePriceLevel.Low => radiatorTemperatures.Moderate,
            RelativePriceLevel.High => radiatorTemperatures.Economic,
            RelativePriceLevel.VeryHigh => radiatorTemperatures.MaxSavings,
            RelativePriceLevel.Extreme => radiatorTemperatures.ExtremeSavings,
            _ => radiatorTemperatures.Baseline,
        };
    }

    private static int GetFloorHeaterTemperature(RelativePriceLevel priceLevel, FloorHeaterTemperatureProfile floorHeaterTemperatures)
    {
        var result = priceLevel switch
        {
            RelativePriceLevel.Normal => floorHeaterTemperatures.Baseline,
            RelativePriceLevel.VeryLow => floorHeaterTemperatures.Enhanced,
            RelativePriceLevel.Low => floorHeaterTemperatures.Moderate,
            RelativePriceLevel.High => floorHeaterTemperatures.Economic,
            RelativePriceLevel.VeryHigh => floorHeaterTemperatures.MaxSavings,
            RelativePriceLevel.Extreme => floorHeaterTemperatures.ExtremeSavings,
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
