using Microsoft.Extensions.Logging;
using MyHome.Core.Interfaces;
using MyHome.Core.PriceCalculations;
using MyHome.Core.Repositories.FloorHeating;
using MyHome.Core.Repositories.HeatPump;
using MyHome.Core.Repositories.WifiSocket;

namespace MyHome.Core.Services;

public class HeatRegulatorService(
    IEnergyRepository energyRepository,
    NibeClient heatpumpReposiory,
    WifiSocketsService wifiSocketsService,
    FloorHeaterRepository floorHeaterRepository,
    ILogger<HeatRegulatorService> logger)
{
    private readonly IEnergyRepository _energyRepository = energyRepository ?? throw new ArgumentNullException(nameof(energyRepository));
    private readonly NibeClient _heatpumpReposiory = heatpumpReposiory ?? throw new ArgumentNullException(nameof(heatpumpReposiory));
    private readonly WifiSocketsService _wifiSocketsService = wifiSocketsService;
    private readonly FloorHeaterRepository _floorHeaterRepository = floorHeaterRepository;
    private readonly ILogger<HeatRegulatorService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task RegulateHeat(CancellationToken cancellationToken = default)
    {
        var prices = await _energyRepository.GetAllAvailableEnergyPrices();
        if (prices.Count == 0)
        {
            _logger.LogWarning("No energy prices available");
            return;
        }

        var price = EnergyPriceCalculator.CreateEneryPrices(prices, DateTime.Now);

        var heatSettings = HeatSettings.CreateFromPriceLevel(price);

        await SetHeat(heatSettings, cancellationToken);
    }

    public async Task SetHeat(HeatSettings settings, CancellationToken cancellationToken)
    {
        var configureHeatPumpTask = ConfigureHeatPump(settings, cancellationToken);
        var updateWifiSocketsTask = _wifiSocketsService.UpdateAllClients(settings.StorageTemprature, cancellationToken);
        var opModeTask = _heatpumpReposiory.UpdateOpMode(settings.OpMode, cancellationToken);
        var updateFloorTempratureTask = _floorHeaterRepository.UpdateSetTemperatureAsync(settings.FloorTemperature);

        await Task.WhenAll(updateWifiSocketsTask, configureHeatPumpTask, updateFloorTempratureTask);
    }

    private async Task ConfigureHeatPump(HeatSettings settings, CancellationToken cancellationToken)
    {
        await _heatpumpReposiory.UpdateHeat(settings.HeatOffset, cancellationToken);
        await _heatpumpReposiory.UpdateComfortMode(settings.ComfortMode, cancellationToken);
    }
}