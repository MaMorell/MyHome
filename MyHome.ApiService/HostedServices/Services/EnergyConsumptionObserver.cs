using AsyncAwaitBestPractices;
using MyHome.ApiService.Constants;
using MyHome.Core.Extensions;
using MyHome.Core.Interfaces;
using MyHome.Core.Models.Entities;
using MyHome.Core.Models.Entities.Profiles;
using MyHome.Core.PriceCalculations;
using MyHome.Core.Services;
using Tibber.Sdk;

namespace MyHome.ApiService.HostedServices.Services;

public sealed class EnergyConsumptionObserver(
    ILogger<EnergyConsumptionObserver> logger,
    IServiceScopeFactory serviceScopeFactory,
    IRepository<EnergyMeasurement> energyMeasurementRepository) : IObserver<RealTimeMeasurement>
{
    private readonly ILogger<EnergyConsumptionObserver> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
    private readonly IRepository<EnergyMeasurement> _energyMeasurementRepository = energyMeasurementRepository;

    private const int HourlyConsumptionLimitKWH = 3;

    public void OnCompleted() => _logger.LogInformation("{Observer} completed", nameof(EnergyConsumptionObserver));

    public void OnError(Exception error) => _logger.LogError(error, "{Observer} failed", nameof(EnergyConsumptionObserver));

    public void OnNext(RealTimeMeasurement value)
    {
        _logger.LogDebug(
            "Current power usage: {Power:N0} W (average: {AveragePower:N0} W); " +
            "Consumption last hour: {Consumption:N3} kWh; " +
            "Cost since last midnight: {Cost:N2} {Currency}",
            value.Power,
            value.AveragePower,
            value.AccumulatedConsumptionLastHour,
            value.AccumulatedCost,
            value.Currency);

        HandleConsumptionLastHour(value.AccumulatedConsumptionLastHour, value.Power)
            .SafeFireAndForget(e => _logger.LogError("Adjust heat failed: {ErrorMessage}", e.Message));
    }

    private async Task HandleConsumptionLastHour(decimal accumulatedConsumptionLastHour, decimal power)
    {
        await _energyMeasurementRepository.UpsertAsync(new EnergyMeasurement
        {
            Id = MyHomeConstants.MyTibberHomeId,
            Power = power,
            AccumulatedConsumptionLastHour = accumulatedConsumptionLastHour,
            UpdatedAt = DateTime.Now
        });

        if (!DateTime.Now.IsWeekdayDayTime())
        {
            return;
        }

        if (accumulatedConsumptionLastHour < HourlyConsumptionLimitKWH)
        {
            return;
        }


        _logger.LogInformation(
            "Accumulated Consumption Last Hour {Consumption} is above the threshold {Threshold} kWh. " +
            "Applying Max savings",
            accumulatedConsumptionLastHour,
            HourlyConsumptionLimitKWH);

        using var scope = _serviceScopeFactory.CreateScope();
        var settingsFactory = scope.ServiceProvider.GetRequiredService<DeviceSettingsFactory>();
        var heatRegulatorService = scope.ServiceProvider.GetRequiredService<HouseAutomationService>();

        var deviceSettings = await settingsFactory.CreateFromMode(DeviceSettingsMode.MaxSavings);
        await heatRegulatorService.AdjustHeatingForCurrentPrice(deviceSettings, CancellationToken.None);
    }
}