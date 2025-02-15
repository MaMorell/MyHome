using MyHome.Core.Models.EnergySupplier;
using MyHome.Core.Repositories.HeatPump.Dtos;

namespace MyHome.Core.PriceCalculations;

public class HeatSettings(
    int heatOffset,
    int storageTemprature,
    ComfortMode comfortMode,
    OpMode opMode,
    int floorTemperature)
{
    public int HeatOffset => heatOffset;
    public ComfortMode ComfortMode => comfortMode;
    public OpMode OpMode => opMode;
    public int StorageTemprature => storageTemprature;
    public int FloorTemperature => floorTemperature;

    public static HeatSettings CreateFromPriceLevel(EnergyPrice price)
    {
        var opMode = HomeConfiguration.GetOpMode(price);
        var heatOffset = HomeConfiguration.GetHeatOffset(price.RelativePriceLevel);
        var targetTemprature = HomeConfiguration.GetRadiatorTemperature(price.RelativePriceLevel);
        var comfortMode = HomeConfiguration.GetComfortMode(price.RelativePriceLevel);
        var floorTemperature = HomeConfiguration.GetFloorHeaterTemperature(price.RelativePriceLevel);

        return new HeatSettings(heatOffset, targetTemprature, comfortMode, opMode, floorTemperature);
    }
}