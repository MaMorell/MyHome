using MyHome.Core.Models.EnergySupplier.Enums;
using MyHome.Core.Repositories.HeatPump.Dtos;

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
        public const ComfortMode Baseline = ComfortMode.Economy;
        public const ComfortMode Enhanced = ComfortMode.Normal;
        public const ComfortMode Moderate = ComfortMode.Normal;
        public const ComfortMode Economic = ComfortMode.Economy;
        public const ComfortMode MaxSavings = ComfortMode.Economy;
        public const ComfortMode ExtremeSavings = ComfortMode.Economy;
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

    public static int GetFloorHeaterTemperature(RelativePriceLevel priceLevel, DateTime currentDate)
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

        result = AdjustTempratureForTimeOfDay(currentDate, result);

        return result;
    }

    private static int AdjustTempratureForTimeOfDay(DateTime currentDate, int result)
    {
        var currentTime = TimeOnly.FromDateTime(currentDate);

        if (IsNightTime(currentTime))
        {
            result -= 4;
        }

        if (IsWeekdayMidDay(currentDate, currentTime))
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

    private static bool IsWeekdayMidDay(DateTime date, TimeOnly time)
    {
        var weekDayMidDayStart = new TimeOnly(10, 0);
        var weekDayMidDayEnd = new TimeOnly(15, 0);

        return
            date.DayOfWeek != DayOfWeek.Saturday &&
            date.DayOfWeek != DayOfWeek.Sunday &&
            time.IsBetween(weekDayMidDayStart, weekDayMidDayEnd);
    }

    private static bool IsNightTime(TimeOnly time)
    {
        var nightStart = new TimeOnly(1, 0);
        var nightEnd = new TimeOnly(4, 0);

        return time.IsBetween(nightStart, nightEnd);
    }
}