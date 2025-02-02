using Microsoft.Extensions.Logging;
using MyHome.Core.Helpers;
using MyHome.Core.Interfaces;
using MyHome.Core.Models.HeatPump;
using MyHome.Core.Repositories.FloorHeating;
using MyHome.Core.Repositories.HeatPump;
using MyHome.Core.Repositories.WifiSocket;

namespace MyHome.Core.Services;

public class HeatResulatorService(
    IEnergyRepository energyRepository,
    HeatpumpClient heatpumpReposiory,
    WifiSocketsService wifiSocketsService,
    FloorHeaterRepository floorHeaterRepository,
    ILogger<HeatResulatorService> logger)
{
    private readonly IEnergyRepository _energyRepository = energyRepository ?? throw new ArgumentNullException(nameof(energyRepository));
    private readonly HeatpumpClient _heatpumpReposiory = heatpumpReposiory ?? throw new ArgumentNullException(nameof(heatpumpReposiory));
    private readonly WifiSocketsService _wifiSocketsService = wifiSocketsService;
    private readonly FloorHeaterRepository _floorHeaterRepository = floorHeaterRepository;
    private readonly ILogger<HeatResulatorService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task RegulateHeat(CancellationToken cancellationToken = default)
    {
        var prices = await _energyRepository.GetEnergyPricesForToday();
        if (prices.Count == 0)
        {
            _logger.LogWarning("No energy prices available for today");
            return;
        }

        var currentHour = DateTime.Now.Hour;
        var price = EnergyPriceCalculator.CreateEneryPrices(prices, currentHour);
        var heatOffset = HomeConfiguration.GetHeatOffset(price.RelativePriceLevel);
        var targetTemprature = HomeConfiguration.GetRadiatorTemperature(price.RelativePriceLevel);
        var comfortMode = HomeConfiguration.GetComfortMode(price.RelativePriceLevel);
        var floorTemperature = HomeConfiguration.GetFloorHeaterTemperature(price.RelativePriceLevel, DateTime.Now);

        _logger.LogInformation(
            "Current energy price {Price:F2} SEK ({Level}). " +
            "Price level considering today's prices: {DayPriceLevel}. " +
            "Heat offset {HeatOffset}. " +
            "Floor temprature {FloorTemperature}. " +
            "Comfort mode {ComfortMode}. " +
            "Target temprature {TargetTemprature}.",
            price.Price, price.PriceLevel, price.RelativePriceLevel, heatOffset, floorTemperature, comfortMode, targetTemprature);

        await SetHeat(heatOffset, targetTemprature, comfortMode, floorTemperature, cancellationToken);
    }

    public async Task SetHeat(int heatOffset, int targetTemprature, ComfortMode comfortMode, int floorTemprature, CancellationToken cancellationToken)
    {
        var updateWifiSocketsTask = _wifiSocketsService.UpdateAllClients(targetTemprature, cancellationToken);
        var updateHeatPumpHeatTask = _heatpumpReposiory.UpdateHeat(heatOffset, cancellationToken);
        var updateComfortModeTask = _heatpumpReposiory.UpdateComfortMode(comfortMode, cancellationToken);
        var updateFloorTempratureTask = _floorHeaterRepository.UpdateSetTemperatureAsync(floorTemprature);

        await Task.WhenAll(updateWifiSocketsTask, updateHeatPumpHeatTask, updateComfortModeTask, updateFloorTempratureTask);
    }
}