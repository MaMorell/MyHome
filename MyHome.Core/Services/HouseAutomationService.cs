using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
    IRepository<DeviceSettingsProfile> deviceSettingsRepository,
    ILogger<HouseAutomationService> logger)
{
    private readonly PriceLevelGenerator _energyPriceCalculator = energyPriceCalculator;
    private readonly IHeatPumpClient _heatPumpClient = heatPumpClient ?? throw new ArgumentNullException(nameof(heatPumpClient));
    private readonly IThermostatClient _bathOneThermostat = bathOneThermostat;
    private readonly IThermostatClient _bathZeroThermostat = bathZeroThermostat;
    private readonly IRepository<DeviceSettingsProfile> _deviceSettingsRepository = deviceSettingsRepository;
    private readonly ILogger<HouseAutomationService> _logger = logger;

    public async Task UpdateDevicesForCurrentPeriod(CancellationToken cancellationToken = default)
    {
        var prices = await _energyPriceCalculator.CreateAsync(EnergyPriceRange.TodayAndTomorrow);
        var priceNow = PriceLevelGenerator.GetForSpecificDate(DateTimeOffset.Now, prices);
        var profile = await _deviceSettingsRepository.GetByIdAsync(EntityIdConstants.DeviceSettingsId)
            ?? throw new EntityNotFoundException(EntityIdConstants.DeviceSettingsId);

        var deviceSettings = DeviceSettingsFactory.CreateFromLevel(priceNow.LevelInternal, profile);
        deviceSettings = await CustomizeDeviceSettings(deviceSettings, prices, profile);

        _logger.LogInformation(
            "PriceNow: StartsAt={StartsAt}, PriceTotal={PriceTotal}, LevelInternal={LevelInternal}, LevelExternal={LevelExternal} | " +
            "DeviceSettings: HeatOffset={HeatOffset}, ComfortMode={ComfortMode}, OpMode={OpMode}, " +
            "StorageTemp={StorageTemp}, BathZeroTemp={BathZeroTemp}, BathOneTemp={BathOneTemp}",
            priceNow.StartsAt,
            priceNow.PriceTotal,
            priceNow.LevelInternal,
            priceNow.LevelExternal,
            deviceSettings.HeatOffset,
            deviceSettings.ComfortMode,
            deviceSettings.OpMode,
            deviceSettings.StorageTemprature,
            deviceSettings.ThermostatBathZeroTemperature,
            deviceSettings.ThermostatBathOneTemperature);

        await ApplyDeviceSettings(deviceSettings, cancellationToken);
    }

    public async Task ApplyDeviceSettings(DeviceSettings deviceSettings, CancellationToken cancellationToken)
    {
        var configureHeatPumpTask = ExecuteDeviceUpdateSafely(
            "heat pump",
            () => ConfigureHeatPump(deviceSettings, cancellationToken));
        //var updateBathZeroThermostatTask = ExecuteDeviceUpdateSafely(
        //  "bath zero thermostat",
        //  () => _bathZeroThermostat.UpdateSetTemperatureAsync(deviceSettings.ThermostatBathZeroTemperature));
        var updateBathOneThermostatTask = ExecuteDeviceUpdateSafely(
            "bath one thermostat",
            () => _bathOneThermostat.UpdateSetTemperatureAsync(deviceSettings.ThermostatBathOneTemperature));

        await Task.WhenAll(configureHeatPumpTask, updateBathOneThermostatTask);
    }

    private async Task ExecuteDeviceUpdateSafely(string deviceName, Func<Task> update)
    {
        try
        {
            await update();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update {DeviceName}", deviceName);
        }
    }

    private static async Task<DeviceSettings> CustomizeDeviceSettings(DeviceSettings settings, IEnumerable<EnergyPriceDetails> prices, DeviceSettingsProfile profile)
    {
        var now = DateTime.Now;

        if (now.IsMidNight() || now.IsMidDay())
        {
            settings.ComfortMode = ComfortMode.Economy;
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

        return settings;
    }

    private static bool ShouldForceManualOpMode(IEnumerable<EnergyPriceDetails> prices, DeviceSettingsProfile profile)
    {
        var priceNow = PriceLevelGenerator.GetForSpecificDate(DateTimeOffset.Now, prices);

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

    private async Task ConfigureHeatPump(DeviceSettings settings, CancellationToken cancellationToken)
    {
        await _heatPumpClient.UpdateHeat(settings.HeatOffset, cancellationToken);
        await _heatPumpClient.UpdateComfortMode(settings.ComfortMode, cancellationToken);
        await _heatPumpClient.UpdateOpMode(settings.OpMode, cancellationToken);
    }
}