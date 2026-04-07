using Microsoft.Extensions.DependencyInjection;
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
    [FromKeyedServices("thermostatBathOne")] IThermostatClient bathOneThermostat,
    [FromKeyedServices("thermostatBathZero")] IThermostatClient bathZeroThermostat,
    IRepository<DeviceSettingsProfile> deviceSettingsRepository)
{
    private readonly PriceLevelGenerator _energyPriceCalculator = energyPriceCalculator;
    private readonly IHeatPumpClient _heatPumpClient = heatPumpClient ?? throw new ArgumentNullException(nameof(heatPumpClient));
    private readonly IThermostatClient _bathOneThermostat = bathOneThermostat;
    private readonly IThermostatClient _bathZeroThermostat = bathZeroThermostat;
    private readonly IRepository<DeviceSettingsProfile> _deviceSettingsRepository = deviceSettingsRepository;

    public async Task UpdateDevicesForCurrentPeriod(CancellationToken cancellationToken = default)
    {
        var prices = await _energyPriceCalculator.CreateAsync(EnergyPriceRange.TodayAndTomorrow);
        var priceNow = PriceLevelGenerator.GetForSpecificDate(DateTime.Now, prices);
        var profile = await _deviceSettingsRepository.GetByIdAsync(EntityIdConstants.DeviceSettingsId)
            ?? throw new EntityNotFoundException(EntityIdConstants.DeviceSettingsId);

        var deviceSettings = DeviceSettingsFactory.CreateFromLevel(priceNow.LevelInternal, profile);
        deviceSettings = await CustomizeDeviceSettings(deviceSettings, prices, profile);

        await ApplyDeviceSettings(deviceSettings, cancellationToken);
        await AdjustVentilationForEvening(cancellationToken);
    }

    public async Task ApplyDeviceSettings(DeviceSettings deviceSettings, CancellationToken cancellationToken)
    {
        var configureHeatPumpTask = ConfigureHeatPump(deviceSettings, cancellationToken);
        //var updateBathZeroThermostatTask = _bathZeroThermostat.UpdateSetTemperatureAsync(deviceSettings.ThermostatBathZeroTemperature);
        var updateBathOneThermostatTask = _bathOneThermostat.UpdateSetTemperatureAsync(deviceSettings.ThermostatBathOneTemperature);

        await Task.WhenAll(configureHeatPumpTask, updateBathOneThermostatTask);
    }

    private async Task<DeviceSettings> CustomizeDeviceSettings(DeviceSettings settings, IEnumerable<EnergyPriceDetails> prices, DeviceSettingsProfile profile)
    {
        var now = DateTime.Now;

        if (now.IsMidNight() || now.IsMidDay())
        {
            settings.ComfortMode = ComfortMode.Economy;
        }

        var exhaustAirTemp = await _heatPumpClient.GetExhaustAirTemp(CancellationToken.None);
        if (exhaustAirTemp >= 22)
        {
            settings.HeatOffset -= 3;
        }

        if (now.IsMidNight())
        {
            settings.ThermostatBathZeroTemperature -= 4;
            settings.ThermostatBathOneTemperature -= 4;
        }
        else if (now.IsWeekdayMidDay() || now.IsEvening())
        {
            settings.ThermostatBathZeroTemperature -= 2;
            settings.ThermostatBathOneTemperature -= 2;
        }

        if (ShouldForceManualOpMode(prices, profile))
        {
            settings.OpMode = OpMode.Manual;
        }
        if (await ShouldForceAutoOpMode())
        {
            settings.OpMode = OpMode.Auto;
        }

        return settings;
    }

    private async Task<bool> ShouldForceAutoOpMode()
    {
        var now = DateTime.Now;
        var nextPeriodicIncrease = await _heatPumpClient.GetNextPeriodicIncrease(CancellationToken.None);
        if (!nextPeriodicIncrease.HasValue)
        {
            return false;
        }

        var targetDate = nextPeriodicIncrease.Value.Date;

        var windowStart = targetDate.AddMinutes(-30);
        var windowEnd = targetDate.AddHours(4);

        return now >= windowStart && now <= windowEnd;
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

    private async Task ConfigureHeatPump(DeviceSettings settings, CancellationToken cancellationToken)
    {
        await _heatPumpClient.UpdateHeat(settings.HeatOffset, cancellationToken);
        await _heatPumpClient.UpdateComfortMode(settings.ComfortMode, cancellationToken);
        await _heatPumpClient.UpdateOpMode(settings.OpMode, cancellationToken);
    }
}