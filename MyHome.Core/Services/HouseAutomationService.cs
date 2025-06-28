using MyHome.Core.Interfaces;
using MyHome.Core.PriceCalculations;

namespace MyHome.Core.Services;

public class HouseAutomationService(
    PriceLevelGenerator energyPriceCalculator,
    IHeatPumpClient heatpumpReposiory,
    IWifiSocketsService wifiSocketsService,
    IFloorHeaterClient floorHeaterRepository,
    DeviceSettingsFactory deviceSettingsCalculator)
{
    private readonly PriceLevelGenerator _energyPriceCalculator = energyPriceCalculator;
    private readonly IHeatPumpClient _heatpumpReposiory = heatpumpReposiory ?? throw new ArgumentNullException(nameof(heatpumpReposiory));
    private readonly IWifiSocketsService _wifiSocketsService = wifiSocketsService;
    private readonly IFloorHeaterClient _floorHeaterRepository = floorHeaterRepository;
    private readonly DeviceSettingsFactory _deviceSettingsCalculator = deviceSettingsCalculator;

    public async Task RegulateHeat(CancellationToken cancellationToken = default)
    {
        var prices = await _energyPriceCalculator.CreateForSpecificDateAsync(DateTime.Now);

        var deviceSettings = await _deviceSettingsCalculator.CreateFromPrice(prices);

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