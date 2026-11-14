using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyHome.Core.Exceptions;
using MyHome.Core.Extensions;
using MyHome.Core.Interfaces;
using MyHome.Core.Models.EnergySupplier.Enums;
using MyHome.Core.Models.Entities.Constants;
using MyHome.Core.Models.Entities.Profiles;
using MyHome.Core.Models.PriceCalculations;
using MyHome.Core.PriceCalculations;

namespace MyHome.Core.Services;

public class HouseAutomationService(
    PriceLevelGenerator energyPriceCalculator,
    IHeatPumpClient heatPumpClient,
    IDehumidifierClient dehumidifierClient,
    [FromKeyedServices("thermostatBathOne")] IThermostatClient bathOneThermostat,
    [FromKeyedServices("thermostatBathZero")] IThermostatClient bathZeroThermostat,
    [FromKeyedServices("thermostatGarage")] IThermostatClient garageThermostat,
    IRepository<DeviceSettingsProfile> deviceSettingsRepository,
    IHomeAssistantEnergyClient homeAssistantEnergyClient,
    ILogger<HouseAutomationService> logger)
{
    private const decimal HourlyConsumptionLimitKWH = 3;

    private readonly PriceLevelGenerator _energyPriceCalculator = energyPriceCalculator;
    private readonly IHeatPumpClient _heatPumpClient = heatPumpClient ?? throw new ArgumentNullException(nameof(heatPumpClient));
    private readonly IDehumidifierClient _dehumidifierClient = dehumidifierClient ?? throw new ArgumentNullException(nameof(dehumidifierClient));
    private readonly IThermostatClient _bathOneThermostat = bathOneThermostat;
    private readonly IThermostatClient _bathZeroThermostat = bathZeroThermostat;
    private readonly IThermostatClient _garageThermostat = garageThermostat;
    private readonly IRepository<DeviceSettingsProfile> _deviceSettingsRepository = deviceSettingsRepository;
    private readonly IHomeAssistantEnergyClient _homeAssistantEnergyClient = homeAssistantEnergyClient ?? throw new ArgumentNullException(nameof(homeAssistantEnergyClient));
    private readonly ILogger<HouseAutomationService> _logger = logger;

    public async Task RunCurrentPeriodAutomationAsync(CancellationToken cancellationToken = default)
    {
        var (prices, profile) = await GetPlanningInputsAsync();

        var now = DateTimeOffset.Now;

        var priceNow = PriceLevelGenerator.GetForSpecificDate(now, prices);

        var deviceSettings = DeviceTargetsPlanner.Create(priceNow, prices, profile, now);
        (deviceSettings, _, _) = await ApplyConsumptionOverride(deviceSettings, profile, now, cancellationToken);

        _logger.LogInformation(
            "PriceNow: StartsAt={StartsAt}, PriceTotal={PriceTotal}, LevelInternal={LevelInternal}, LevelExternal={LevelExternal} | " +
            "DeviceSettings: HeatOffset={HeatOffset}, ComfortMode={ComfortMode}, OpMode={OpMode}, " +
            "BathZeroTemp={BathZeroTemp}, BathOneTemp={BathOneTemp}, GarageTemp={GarageTemp}, DehumidifierTargetHumidity={DehumidifierTargetHumidity}",
            priceNow.StartsAt,
            priceNow.PriceTotal,
            priceNow.LevelInternal,
            priceNow.LevelExternal,
            deviceSettings.HeatOffset,
            deviceSettings.ComfortMode,
            deviceSettings.OpMode,
            deviceSettings.ThermostatBathZeroTemperature,
            deviceSettings.ThermostatBathOneTemperature,
            deviceSettings.ThermostatGarageTemperature,
            deviceSettings.DehumidifierTargetHumidity);

        await ApplyDeviceSettings(deviceSettings, cancellationToken);
    }

    private async Task<(IReadOnlyList<EnergyPriceDetails> Prices, DeviceSettingsProfile Profile)> GetPlanningInputsAsync()
    {
        var prices = (await _energyPriceCalculator.CreateAsync(EnergyPriceRange.TodayAndTomorrow))
            .OrderBy(price => price.StartsAt)
            .ToList();
        var profile = await _deviceSettingsRepository.GetByIdAsync(EntityIdConstants.DeviceSettingsId)
            ?? throw new EntityNotFoundException(EntityIdConstants.DeviceSettingsId);
        return (prices, profile);
    }

    private async Task ApplyDeviceSettings(DeviceTargets deviceSettings, CancellationToken cancellationToken)
    {
        var configureHeatPumpTask = ExecuteDeviceUpdateSafely(
            "heat pump",
            () => ConfigureHeatPump(deviceSettings, cancellationToken));
        var updateBathZeroThermostatTask = ExecuteDeviceUpdateSafely(
            "bath zero thermostat",
            () => _bathZeroThermostat.UpdateSetTemperatureAsync(deviceSettings.ThermostatBathZeroTemperature));
        var updateBathOneThermostatTask = ExecuteDeviceUpdateSafely(
            "bath one thermostat",
            () => _bathOneThermostat.UpdateSetTemperatureAsync(deviceSettings.ThermostatBathOneTemperature));
        var updateGarageThermostatTask = ExecuteDeviceUpdateSafely(
            "garage thermostat",
            () => _garageThermostat.UpdateSetTemperatureAsync(deviceSettings.ThermostatGarageTemperature));
        var configureDehumidifierTask = ExecuteDeviceUpdateSafely(
            "dehumidifier",
            () => _dehumidifierClient.SetTargetHumidityAsync(deviceSettings.DehumidifierTargetHumidity, cancellationToken));

        await Task.WhenAll(
            configureHeatPumpTask,
            updateBathZeroThermostatTask,
            updateBathOneThermostatTask,
            updateGarageThermostatTask,
            configureDehumidifierTask);
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

    private async Task<(DeviceTargets Settings, ConsumptionOverrideStatus Status, decimal? Consumption)> ApplyConsumptionOverride(
        DeviceTargets settings,
        DeviceSettingsProfile profile,
        DateTimeOffset at,
        CancellationToken cancellationToken,
        bool readOutsideOverrideWindow = false)
    {
        var overrideApplicable = at.DateTime.IsWeekdayDayTime();
        if (!overrideApplicable && !readOutsideOverrideWindow)
        {
            return (settings, ConsumptionOverrideStatus.NotApplicable, null);
        }

        decimal accumulatedConsumptionCurrentHour;
        try
        {
            accumulatedConsumptionCurrentHour = await _homeAssistantEnergyClient
                .GetAccumulatedConsumptionCurrentHourAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read accumulated consumption from Home Assistant");
            return (settings, overrideApplicable ? ConsumptionOverrideStatus.Unavailable : ConsumptionOverrideStatus.NotApplicable, null);
        }

        if (!overrideApplicable)
        {
            return (settings, ConsumptionOverrideStatus.NotApplicable, accumulatedConsumptionCurrentHour);
        }

        if (accumulatedConsumptionCurrentHour < HourlyConsumptionLimitKWH)
        {
            return (settings, ConsumptionOverrideStatus.BelowLimit, accumulatedConsumptionCurrentHour);
        }

        _logger.LogInformation(
            "Accumulated Consumption Current Hour {Consumption} is above the threshold {Threshold} kWh. " +
            "Applying Max savings",
            accumulatedConsumptionCurrentHour,
            HourlyConsumptionLimitKWH);

        return (DeviceTargetsFactory.CreateFromLevel(EnergyPriceLevel.VeryExpensive, profile),
            ConsumptionOverrideStatus.Applied,
            accumulatedConsumptionCurrentHour);
    }

    private async Task ConfigureHeatPump(DeviceTargets settings, CancellationToken cancellationToken)
    {
        await _heatPumpClient.UpdateHeat(settings.HeatOffset, cancellationToken);
        await _heatPumpClient.UpdateComfortMode(settings.ComfortMode, cancellationToken);
        await _heatPumpClient.UpdateOpMode(settings.OpMode, cancellationToken);
    }
}