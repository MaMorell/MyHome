using Microsoft.Extensions.Logging;
using MyHome.Core.Helpers;
using MyHome.Core.Interfaces;
using MyHome.Core.Models;
using MyHome.Core.Repositories;

namespace MyHome.Core;

public class HeatResulatorService(
    IEnergyRepository energyRepository,
    HeatpumpClient heatpumpReposiory,
    WifiSocketsService wifiSocketsService,
    ILogger<HeatResulatorService> logger)
{
    private readonly IEnergyRepository _energyRepository = energyRepository ?? throw new ArgumentNullException(nameof(energyRepository));
    private readonly HeatpumpClient _heatpumpReposiory = heatpumpReposiory ?? throw new ArgumentNullException(nameof(heatpumpReposiory));
    private readonly WifiSocketsService _wifiSocketsService = wifiSocketsService;
    private readonly ILogger<HeatResulatorService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task RegulateHeat(CancellationToken cancellationToken = default)
    {
        var prices = await _energyRepository.GetTodaysEnergyPrices();
        if (prices.Count == 0)
        {
            _logger.LogWarning("No energy prices available for today");
            return;
        }

        var currentHour = DateTime.Now.Hour;
        var price = HeatRegulator.CreateEneryPrices(prices, currentHour);
        var heatOffset = HomeConfiguration.GetHeatOffset(price.RelativePriceLevel);
        var targetTemprature = HomeConfiguration.GetTargetTemperature(price.RelativePriceLevel);
        var comfortMode = HomeConfiguration.GetComfortMode(price.RelativePriceLevel);

        _logger.LogInformation(
            "Current energy price {Price:F2} SEK ({Level}). " +
            "Price level considering today's prices: {DayPriceLevel}. " +
            "Heat offset {HeatOffset}. " +
            "Comfort mode {ComfortMode}. " +
            "Target temprature {TargetTemprature}.",
            price.Price, price.PriceLevel, price.RelativePriceLevel, heatOffset, comfortMode, targetTemprature);

        await SetHeat(heatOffset, targetTemprature, comfortMode, cancellationToken);
    }

    public async Task SetHeat(int heatOffset, int targetTemprature, ComfortMode comfortMode, CancellationToken cancellationToken)
    {
        var updateWifiSocketsTask = _wifiSocketsService.UpdateAllClients(targetTemprature, cancellationToken);
        var updateHeatPumpHeatTask = _heatpumpReposiory.UpdateHeat(heatOffset, cancellationToken);
        var updateComfortModeTask = _heatpumpReposiory.UpdateComfortMode(comfortMode, cancellationToken);

        await Task.WhenAll(updateWifiSocketsTask, updateHeatPumpHeatTask, updateComfortModeTask);
    }
}