using MyHome.Core.Models.Integrations.HeatPump;

namespace MyHome.Core.PriceCalculations;

public class DeviceSettings(
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
}