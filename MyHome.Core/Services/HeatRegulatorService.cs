using Microsoft.Extensions.Logging;
using MyHome.Core.Interfaces;
using MyHome.Core.Models.EnergySupplier.Enums;
using MyHome.Core.PriceCalculations;

namespace MyHome.Core.Services;

public class HeatRegulatorService(
    IEnergySupplierRepository energyRepository,
    EnergyPriceCalculator energyPriceCalculator,
    IHeatPumpClient heatpumpReposiory,
    IWifiSocketsService wifiSocketsService,
    IFloorHeaterClient floorHeaterRepository,
    DeviceSettingsFactory deviceSettingsCalculator,
    ILogger<HeatRegulatorService> logger)
{
    private readonly IEnergySupplierRepository _energyRepository = energyRepository ?? throw new ArgumentNullException(nameof(energyRepository));
    private readonly EnergyPriceCalculator _energyPriceCalculator = energyPriceCalculator;
    private readonly IHeatPumpClient _heatpumpReposiory = heatpumpReposiory ?? throw new ArgumentNullException(nameof(heatpumpReposiory));
    private readonly IWifiSocketsService _wifiSocketsService = wifiSocketsService;
    private readonly IFloorHeaterClient _floorHeaterRepository = floorHeaterRepository;
    private readonly DeviceSettingsFactory _deviceSettingsCalculator = deviceSettingsCalculator;
    private readonly ILogger<HeatRegulatorService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task RegulateHeat(CancellationToken cancellationToken = default)
    {
        var prices = await _energyRepository.GetEnergyPrices(PriceType.All);
        if (prices.Count == 0)
        {
            _logger.LogWarning("No energy prices available");
            return;
        }

        var price = await _energyPriceCalculator.CreateEneryPrices(prices, DateTime.Now);

        var deviceSettings = await _deviceSettingsCalculator.CreateFromPrice(price);

        await SetHeat(deviceSettings, cancellationToken);
    }

    public async Task SetHeat(DeviceSettings settings, CancellationToken cancellationToken)
    {
        var configureHeatPumpTask = ConfigureHeatPump(settings, cancellationToken);
        var updateWifiSocketsTask = _wifiSocketsService.UpdateAllClients(settings.StorageTemprature, cancellationToken);
        var opModeTask = _heatpumpReposiory.UpdateOpMode(settings.OpMode, cancellationToken);
        var updateFloorTempratureTask = _floorHeaterRepository.UpdateSetTemperatureAsync(settings.FloorTemperature);

        await Task.WhenAll(updateWifiSocketsTask, configureHeatPumpTask, opModeTask, updateFloorTempratureTask);
    }

    private async Task ConfigureHeatPump(DeviceSettings settings, CancellationToken cancellationToken)
    {
        await _heatpumpReposiory.UpdateHeat(settings.HeatOffset, cancellationToken);
        await _heatpumpReposiory.UpdateComfortMode(settings.ComfortMode, cancellationToken);
    }
}