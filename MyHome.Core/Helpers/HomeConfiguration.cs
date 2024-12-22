using MyHome.Core.Models.EnergySupplier;
using MyHome.Core.Models.HeatPump;

namespace MyHome.Core.Helpers;

public static class HomeConfiguration
{
    public static class HeatOffsets
    {
        public const int Baseline = 0;
        public const int Enhanced = 2;
        public const int Moderate = 1;
        public const int Economic = -1;
        public const int MaxSavings = -2;
        public const int ExtremeSavings = -5;
    }

    public static class ComfortModes
    {
        public const ComfortMode Baseline = ComfortMode.Normal;
        public const ComfortMode Enhanced = ComfortMode.Normal;
        public const ComfortMode Moderate = ComfortMode.Normal;
        public const ComfortMode Economic = ComfortMode.Economy;
        public const ComfortMode MaxSavings = ComfortMode.Economy;
        public const ComfortMode ExtremeSavings = ComfortMode.Economy;
    }

    public static class Temperatures
    {
        public const int Baseline = 7;
        public const int Enhanced = 10;
        public const int Moderate = 8;
        public const int Economic = 5;
        public const int MaxSavings = 5;
        public const int ExtremeSavings = 5;
    }

    public static int GetHeatOffset(RelativePriceLevel priceLevel) => priceLevel switch
    {
        RelativePriceLevel.Normal => HeatOffsets.Baseline,
        RelativePriceLevel.VeryLow => HeatOffsets.Enhanced,
        RelativePriceLevel.Low => HeatOffsets.Moderate,
        RelativePriceLevel.High => HeatOffsets.Economic,
        RelativePriceLevel.VeryHigh => HeatOffsets.MaxSavings,
        RelativePriceLevel.Extreme => HeatOffsets.ExtremeSavings,
        _ => HeatOffsets.Baseline,
    };

    public static ComfortMode GetComfortMode(RelativePriceLevel priceLevel) => priceLevel switch
    {
        RelativePriceLevel.Normal => ComfortModes.Baseline,
        RelativePriceLevel.VeryLow => ComfortModes.Enhanced,
        RelativePriceLevel.Low => ComfortModes.Moderate,
        RelativePriceLevel.High => ComfortModes.Economic,
        RelativePriceLevel.VeryHigh => ComfortModes.MaxSavings,
        RelativePriceLevel.Extreme => ComfortModes.ExtremeSavings,
        _ => ComfortModes.Baseline,
    };

    public static int GetTargetTemperature(RelativePriceLevel priceLevel) => priceLevel switch
    {
        RelativePriceLevel.Normal => Temperatures.Baseline,
        RelativePriceLevel.VeryLow => Temperatures.Enhanced,
        RelativePriceLevel.Low => Temperatures.Moderate,
        RelativePriceLevel.High => Temperatures.Economic,
        RelativePriceLevel.VeryHigh => Temperatures.MaxSavings,
        RelativePriceLevel.Extreme => Temperatures.ExtremeSavings,
        _ => Temperatures.Baseline,
    };
}