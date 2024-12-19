using AsyncAwaitBestPractices;
using MyHome.Core;
using MyHome.Core.Helpers;
using Tibber.Sdk;

namespace MyHome.ApiService.HostedServices.Services;

public sealed class EnergyConsumptionObserver : IObserver<RealTimeMeasurement>
{
    private readonly ILogger<EnergyConsumptionObserver> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private DateTime _lastEnergyPriceAdjustmentCheck = DateTime.MinValue;
    private const int LastHourConsumptionLimitKWH = 3;
    private const int MinimumMinutesBetweenChecks = 10;

    public EnergyConsumptionObserver(
        ILogger<EnergyConsumptionObserver> logger,
        IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
    }

    public void OnCompleted()
    {
        _logger.LogInformation("{Observer} completed", nameof(EnergyConsumptionObserver));
    }

    public void OnError(Exception error)
    {
        _logger.LogError(error.Message);
    }

    public void OnNext(RealTimeMeasurement value)
    {
        var minutesSinceLastCheck = (DateTime.Now - _lastEnergyPriceAdjustmentCheck).Minutes;
        if (minutesSinceLastCheck < MinimumMinutesBetweenChecks)
        {
            _logger.LogDebug(
                "Less than {MinimumMinutes} minutes passed since last energy price check. " +
                "Current time: {CurrentTime}. Last check: {LastCheck}. " +
                "Minutes since last check: {MinutesPassed}",
                MinimumMinutesBetweenChecks,
                DateTime.Now,
                _lastEnergyPriceAdjustmentCheck,
                minutesSinceLastCheck);
            return;
        }

        _logger.LogInformation(
            "Current power usage: {Power:N0} W (average: {AveragePower:N0} W); " +
            "Consumption last hour: {Consumption:N3} kWh; " +
            "Cost since last midnight: {Cost:N2} {Currency}",
            value.Power,
            value.AveragePower,
            value.AccumulatedConsumptionLastHour,
            value.AccumulatedCost,
            value.Currency);

        HandleConsumptionLastHour(value.AccumulatedConsumptionLastHour)
            .SafeFireAndForget(e => _logger.LogError("Adjust heat failed: {ErrorMessage}", e.Message));
    }

    private async Task HandleConsumptionLastHour(decimal accumulatedConsumptionLastHour)
    {
        try
        {
            if (accumulatedConsumptionLastHour < LastHourConsumptionLimitKWH)
            {
                return;
            }

            using var scope = _serviceScopeFactory.CreateScope();
            var heatRegulatorService = scope.ServiceProvider.GetRequiredService<HeatResulatorService>();

            _logger.LogInformation(
                "Accumulated Consumption Last Hour {Consumption} is above the threshold {Threshold} kWh. " +
                "Applying Max savings",
                accumulatedConsumptionLastHour,
                LastHourConsumptionLimitKWH);

            await heatRegulatorService.SetHeat(
                HomeConfiguration.HeatOffsets.Economic,
                HomeConfiguration.Temperatures.Economic,
                HomeConfiguration.ComfortModes.Economic,
                CancellationToken.None);

            _lastEnergyPriceAdjustmentCheck = DateTime.Now;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Heat regulation cycle failed");
            throw;
        }
    }
}