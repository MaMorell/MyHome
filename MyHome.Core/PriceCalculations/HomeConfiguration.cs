using MyHome.Core.Models.EnergySupplier;
using MyHome.Core.Models.EnergySupplier.Enums;
using MyHome.Core.Repositories.HeatPump.Dtos;
using System.ComponentModel.Design;
using Tibber.Sdk;
using static MyHome.Core.PriceCalculations.HomeConfiguration;
using PriceLevel = MyHome.Core.Models.EnergySupplier.Enums.PriceLevel;

namespace MyHome.Core.PriceCalculations;

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
        public const ComfortMode Baseline = ComfortMode.Economy;
        public const ComfortMode Enhanced = ComfortMode.Normal;
        public const ComfortMode Moderate = ComfortMode.Normal;
        public const ComfortMode Economic = ComfortMode.Economy;
        public const ComfortMode MaxSavings = ComfortMode.Economy;
        public const ComfortMode ExtremeSavings = ComfortMode.Economy;
    }

    public static class OpModes
    {
        public const OpMode Baseline = OpMode.Auto;
        public const OpMode Enhanced = OpMode.Auto;
        public const OpMode Moderate = OpMode.Auto;
        public const OpMode Economic = OpMode.Manual;
        public const OpMode MaxSavings = OpMode.Manual;
        public const OpMode ExtremeSavings = OpMode.Manual;
    }

    public static class RadiatorTemperatures
    {
        public const int Baseline = 8;
        public const int Enhanced = 12;
        public const int Moderate = 10;
        public const int Economic = 6;
        public const int MaxSavings = 5;
        public const int ExtremeSavings = 5;
    }

    public static class FloorHeaterTemperatures
    {
        public const int Baseline = 22;
        public const int Enhanced = 25;
        public const int Moderate = 24;
        public const int Economic = 15;
        public const int MaxSavings = 10;
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

    public static OpMode GetOpMode(EnergyPrice energyPrice)
    {
        var priceLevelIsCheap = 
            energyPrice.PriceLevel == PriceLevel.Cheap || 
            energyPrice.PriceLevel == PriceLevel.VeryCheap;

        var priceLimit = DateTime.Now.IsNightTime() 
            ? 1.2m 
            : 1.0m;
        var priceIsCheap = energyPrice.Price < priceLimit;

        if (priceLevelIsCheap && priceIsCheap)
        {
            return OpMode.Auto;
        }
        else
        {
            return OpMode.Manual;
        }
    }

    public static int GetRadiatorTemperature(RelativePriceLevel priceLevel) => priceLevel switch
    {
        RelativePriceLevel.Normal => RadiatorTemperatures.Baseline,
        RelativePriceLevel.VeryLow => RadiatorTemperatures.Enhanced,
        RelativePriceLevel.Low => RadiatorTemperatures.Moderate,
        RelativePriceLevel.High => RadiatorTemperatures.Economic,
        RelativePriceLevel.VeryHigh => RadiatorTemperatures.MaxSavings,
        RelativePriceLevel.Extreme => RadiatorTemperatures.ExtremeSavings,
        _ => RadiatorTemperatures.Baseline,
    };

    public static int GetFloorHeaterTemperature(RelativePriceLevel priceLevel)
    {
        var result = priceLevel switch
        {
            RelativePriceLevel.Normal => FloorHeaterTemperatures.Baseline,
            RelativePriceLevel.VeryLow => FloorHeaterTemperatures.Enhanced,
            RelativePriceLevel.Low => FloorHeaterTemperatures.Moderate,
            RelativePriceLevel.High => FloorHeaterTemperatures.Economic,
            RelativePriceLevel.VeryHigh => FloorHeaterTemperatures.MaxSavings,
            RelativePriceLevel.Extreme => FloorHeaterTemperatures.ExtremeSavings,
            _ => FloorHeaterTemperatures.Baseline,
        };

        result = AdjustTempratureForTimeOfDay(result);

        return result;
    }

    private static int AdjustTempratureForTimeOfDay(int result)
    {
        var now = DateTime.Now;

        if (now.IsNightTime())
        {
            result -= 4;
        }

        if (now.IsWeekdayMidDay())
        {
            result -= 3;
        }

        const int MinAllowedTemperature = 5;
        if (result < MinAllowedTemperature)
        {
            result = 5;
        }

        return result;
    }
}
