using MyHome.Core.Models.Integrations.HeatPump;

namespace MyHome.Core.PriceCalculations;

public class DeviceSettings(
    int heatOffset,
    int storageTemprature,
    ComfortMode comfortMode,
    OpMode opMode,
    int floorTemperature)
{
    public int HeatOffset { get; set; } = heatOffset;
    public ComfortMode ComfortMode { get; set; } = comfortMode;
    public OpMode OpMode { get; set; } = opMode;
    public int StorageTemprature { get; set; } = storageTemprature;
    public int FloorTemperature { get; set; } = floorTemperature;
}