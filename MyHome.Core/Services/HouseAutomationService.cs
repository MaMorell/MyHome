using MyHome.Core.Interfaces;
using MyHome.Core.Models.Integrations.HeatPump;
using MyHome.Core.PriceCalculations;

namespace MyHome.Core.Services;

public class HouseAutomationService(
    PriceLevelGenerator energyPriceCalculator,
    IHeatPumpClient heatpumpClient,
    IWifiSocketsService wifiSocketsService,
    IFloorHeaterClient floorHeaterRepository,
    DeviceSettingsFactory deviceSettingsCalculator)
{
    private readonly PriceLevelGenerator _energyPriceCalculator = energyPriceCalculator;
    private readonly IHeatPumpClient _heatpumpClient = heatpumpClient ?? throw new ArgumentNullException(nameof(heatpumpClient));
    private readonly IWifiSocketsService _wifiSocketsService = wifiSocketsService;
    private readonly IFloorHeaterClient _floorHeaterRepository = floorHeaterRepository;
    private readonly DeviceSettingsFactory _deviceSettingsCalculator = deviceSettingsCalculator;

    public async Task UpdateHouseSettings(CancellationToken cancellationToken = default)
    {
        await AdjustHeatingForCurrentPrice(cancellationToken);
        await AdjustVentilationForEvening(cancellationToken);
    }

    private async Task AdjustVentilationForEvening(CancellationToken cancellationToken)
    {
        if (DateTime.Now.Hour == 19)
        {
            var outdoorTemp = await _heatpumpClient.GetCurrentOutdoorTemp(cancellationToken);
            var exhaustAirTemp = await _heatpumpClient.GetExhaustAirTemp(cancellationToken);
            var diff = exhaustAirTemp - outdoorTemp;
            if (exhaustAirTemp > 22 && diff > 2)
            {
                await _heatpumpClient.UpdateIncreasedVentilation(IncreasedVentilationValue.On, cancellationToken);
            }
        }
        else
        {
            await _heatpumpClient.UpdateIncreasedVentilation(IncreasedVentilationValue.Off, cancellationToken);
        }
    }

    public async Task AdjustHeatingForCurrentPrice(CancellationToken cancellationToken)
    {
        var prices = await _energyPriceCalculator.CreateForSpecificDateAsync(DateTime.Now);
        var deviceSettings = await _deviceSettingsCalculator.CreateFromPrice(prices);

        var configureHeatPumpTask = ConfigureHeatPump(deviceSettings, cancellationToken);
        var updateWifiSocketsTask = _wifiSocketsService.UpdateAllClients(deviceSettings.StorageTemprature, cancellationToken);
        var opModeTask = _heatpumpClient.UpdateOpMode(deviceSettings.OpMode, cancellationToken);
        var updateFloorTempratureTask = _floorHeaterRepository.UpdateSetTemperatureAsync(deviceSettings.FloorTemperature);

        await Task.WhenAll(updateWifiSocketsTask, configureHeatPumpTask, opModeTask, updateFloorTempratureTask);
    }

    private async Task ConfigureHeatPump(DeviceSettings settings, CancellationToken cancellationToken)
    {
        await _heatpumpClient.UpdateHeat(settings.HeatOffset, cancellationToken);
        await _heatpumpClient.UpdateComfortMode(settings.ComfortMode, cancellationToken);
    }
}