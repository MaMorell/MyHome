using MyHome.Core.Exceptions;
using MyHome.Core.Extensions;
using MyHome.Core.Interfaces;
using MyHome.Core.Models.EnergySupplier.Enums;
using MyHome.Core.Models.Entities.Constants;
using MyHome.Core.Models.Entities.Profiles;
using MyHome.Core.Models.Integrations.HeatPump;
using MyHome.Core.Models.PriceCalculations;
using MyHome.Core.PriceCalculations;

namespace MyHome.Core.Services;

public class HouseAutomationService(
    PriceLevelGenerator energyPriceCalculator,
    IHeatPumpClient heatPumpClient,
    IFloorHeaterClient floorHeaterRepository,
    IRepository<DeviceSettingsProfile> deviceSettingsRepository)
{
    private readonly PriceLevelGenerator _energyPriceCalculator = energyPriceCalculator;
    private readonly IHeatPumpClient _heatPumpClient = heatPumpClient ?? throw new ArgumentNullException(nameof(heatPumpClient));
    private readonly IFloorHeaterClient _floorHeaterRepository = floorHeaterRepository;
    private readonly IRepository<DeviceSettingsProfile> _deviceSettingsRepository = deviceSettingsRepository;

    public async Task UpdateHouseSettings(CancellationToken cancellationToken = default)
    {
        var prices = await _energyPriceCalculator.CreateAsync(EnergyPriceRange.TodayAndTomorrow);
        var priceNow = PriceLevelGenerator.GetForSpecificDate(DateTime.Now, prices);
        var profile = await _deviceSettingsRepository.GetByIdAsync(EntityIdConstants.DeviceSettingsId)
            ?? throw new EntityNotFoundException(EntityIdConstants.DeviceSettingsId);

        var deviceSettings = DeviceSettingsFactory.CreateFromLevel(priceNow.LevelInternal, profile);
        deviceSettings = await CustomizeDeviceSettings(deviceSettings, prices, profile);

        await AdjustHeatingForCurrentPrice(deviceSettings, cancellationToken);
        await AdjustVentilationForEvening(cancellationToken);
    }

    private async Task<DeviceSettings> CustomizeDeviceSettings(DeviceSettings settings, IEnumerable<EnergyPriceDetails> prices, DeviceSettingsProfile profile)
    {
        var now = DateTime.Now;

        if (now.IsMidNight() || now.IsMidDay())
        {
            settings.ComfortMode = ComfortMode.Economy;
        }

        var exhaustAirTemp = await _heatPumpClient.GetExhaustAirTemp(CancellationToken.None);
        if (exhaustAirTemp >= 21)
        {
            settings.HeatOffset -= 3;
        }

        if (now.IsMidNight())
        {
            settings.FloorTemperature -= 4;
        }
        else if (now.IsWeekdayMidDay() || now.IsEvening())
        {
            settings.FloorTemperature -= 2;
        }

        if (ShouldForceManualOpMode(prices, profile))
        {
            settings.OpMode = OpMode.Manual;
        }

        return settings;
    }

    private static bool ShouldForceManualOpMode(IEnumerable<EnergyPriceDetails> prices, DeviceSettingsProfile profile)
    {
        var priceNow = PriceLevelGenerator.GetForSpecificDate(DateTime.Now, prices);

        if (priceNow.StartsAt.DateTime.IsWeekdayDayTime())
        {
            return true;
        }

        if (priceNow.PriceTotal > profile.OpModes.MaxPriceAutoMode)
        {
            return true;
        }

        var nonMidayPrices = prices.Where(p => !p.StartsAt.DateTime.IsWeekdayDayTime()).ToList();
        var lowestSixthThreshold = nonMidayPrices
            .OrderBy(p => p.PriceTotal)
            .ElementAtOrDefault(nonMidayPrices.Count / 6);

        if (lowestSixthThreshold == null || priceNow.PriceTotal > lowestSixthThreshold.PriceTotal)
        {
            return true;
        }

        return false;
    }

    private async Task AdjustVentilationForEvening(CancellationToken cancellationToken)
    {
        if (DateTime.Now.Hour == 18 || DateTime.Now.Hour == 19)
        {
            var outdoorTemp = await _heatPumpClient.GetCurrentOutdoorTemp(cancellationToken);
            var exhaustAirTemp = await _heatPumpClient.GetExhaustAirTemp(cancellationToken);
            var diff = exhaustAirTemp - outdoorTemp;
            if (exhaustAirTemp >= 25 && diff >= 6)
            {
                await _heatPumpClient.UpdateIncreasedVentilation(IncreasedVentilationValue.On, cancellationToken);
            }
        }
        else
        {
            await _heatPumpClient.UpdateIncreasedVentilation(IncreasedVentilationValue.Off, cancellationToken);
        }
    }

    public async Task AdjustHeatingForCurrentPrice(DeviceSettings deviceSettings, CancellationToken cancellationToken)
    {
        var configureHeatPumpTask = ConfigureHeatPump(deviceSettings, cancellationToken);
        var updateFloorTempratureTask = _floorHeaterRepository.UpdateSetTemperatureAsync(deviceSettings.FloorTemperature);

        await Task.WhenAll(configureHeatPumpTask, updateFloorTempratureTask);
    }

    private async Task ConfigureHeatPump(DeviceSettings settings, CancellationToken cancellationToken)
    {
        await _heatPumpClient.UpdateHeat(settings.HeatOffset, cancellationToken);
        await _heatPumpClient.UpdateComfortMode(settings.ComfortMode, cancellationToken);
        await _heatPumpClient.UpdateOpMode(settings.OpMode, cancellationToken);
    }
}