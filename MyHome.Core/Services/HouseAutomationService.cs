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
        var prices = await _energyPriceCalculator.CreateForSpecificDateAsync(DateTime.Now);
        var deviceSettings = await _deviceSettingsCalculator.CreateFromPrice(prices);
        await AdjustHeatingForCurrentPrice(deviceSettings, cancellationToken);

        await AdjustVentilationForEvening(cancellationToken);
    }

    private async Task AdjustVentilationForEvening(CancellationToken cancellationToken)
    {
        if (DateTime.Now.Hour == 18 || DateTime.Now.Hour == 19)
        {
            var outdoorTemp = await _heatpumpClient.GetCurrentOutdoorTemp(cancellationToken);
            var exhaustAirTemp = await _heatpumpClient.GetExhaustAirTemp(cancellationToken);
            var diff = exhaustAirTemp - outdoorTemp;
            if (exhaustAirTemp >= 24 && diff >= 5)
            {
                await _heatpumpClient.UpdateIncreasedVentilation(IncreasedVentilationValue.On, cancellationToken);
            }
        }
        else
        {
            await _heatpumpClient.UpdateIncreasedVentilation(IncreasedVentilationValue.Off, cancellationToken);
        }
    }

    public async Task AdjustHeatingForCurrentPrice(DeviceSettings deviceSettings, CancellationToken cancellationToken)
    {
        var configureHeatPumpTask = ConfigureHeatPump(deviceSettings, cancellationToken);
        //var updateWifiSocketsTask = _wifiSocketsService.UpdateAllClients(deviceSettings.StorageTemprature, cancellationToken);
        var opModeTask = _heatpumpClient.UpdateOpMode(deviceSettings.OpMode, cancellationToken);
        var updateFloorTempratureTask = _floorHeaterRepository.UpdateSetTemperatureAsync(deviceSettings.FloorTemperature);

        await Task.WhenAll(configureHeatPumpTask, opModeTask, updateFloorTempratureTask);
    }

    private async Task ConfigureHeatPump(DeviceSettings settings, CancellationToken cancellationToken)
    {
        await _heatpumpClient.UpdateHeat(settings.HeatOffset, cancellationToken);
        await _heatpumpClient.UpdateComfortMode(settings.ComfortMode, cancellationToken);
    }
}