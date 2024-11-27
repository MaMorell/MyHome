using MyHome.Core.Models;

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
    }

    public static class ComfortModes
    {
        public const ComfortMode Baseline = ComfortMode.Normal;
        public const ComfortMode Enhanced = ComfortMode.Luxury;
        public const ComfortMode Moderate = ComfortMode.Normal;
        public const ComfortMode Economic = ComfortMode.Economy;
        public const ComfortMode MaxSavings = ComfortMode.Economy;
    }

    public static class Temperatures
    {
        public const int Baseline = 7;
        public const int Enhanced = 12;
        public const int Moderate = 10;
        public const int Economic = 5;
        public const int MaxSavings = 5;
    }

    public static int GetHeatOffset(RelativePriceLevel priceLevel) => priceLevel switch
    {
        RelativePriceLevel.Normal => HeatOffsets.Baseline,
        RelativePriceLevel.VeryLow => HeatOffsets.Enhanced,
        RelativePriceLevel.Low => HeatOffsets.Moderate,
        RelativePriceLevel.High => HeatOffsets.Economic,
        RelativePriceLevel.VeryHigh => HeatOffsets.MaxSavings,
        _ => HeatOffsets.Baseline,
    };

    public static ComfortMode GetComfortMode(RelativePriceLevel priceLevel) => priceLevel switch
    {
        RelativePriceLevel.Normal => ComfortModes.Baseline,
        RelativePriceLevel.VeryLow => ComfortModes.Enhanced,
        RelativePriceLevel.Low => ComfortModes.Moderate,
        RelativePriceLevel.High => ComfortModes.Economic,
        RelativePriceLevel.VeryHigh => ComfortModes.MaxSavings,
        _ => ComfortModes.Baseline,
    };

    public static int GetTargetTemperature(RelativePriceLevel priceLevel) => priceLevel switch
    {
        RelativePriceLevel.Normal => Temperatures.Baseline,
        RelativePriceLevel.VeryLow => Temperatures.Enhanced,
        RelativePriceLevel.Low => Temperatures.Moderate,
        RelativePriceLevel.High => Temperatures.Economic,
        RelativePriceLevel.VeryHigh => Temperatures.MaxSavings,
        _ => Temperatures.Baseline,
    };
}