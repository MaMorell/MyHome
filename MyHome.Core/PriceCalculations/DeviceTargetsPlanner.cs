using MyHome.Core.Extensions;
using MyHome.Core.Models.Entities.Profiles;
using MyHome.Core.Models.Integrations.HeatPump;
using MyHome.Core.Models.PriceCalculations;

namespace MyHome.Core.PriceCalculations;

public static class DeviceTargetsPlanner
{
    public static DeviceTargets Create(
        EnergyPriceDetails price,
        IReadOnlyList<EnergyPriceDetails> prices,
        DeviceSettingsProfile profile,
        DateTimeOffset at)
    {
        var settings = DeviceTargetsFactory.CreateFromLevel(price.LevelInternal, profile);
        var localTime = at.DateTime;

        if (localTime.IsMidNight() || localTime.IsMidDay())
        {
            settings = settings with { ComfortMode = ComfortMode.Economy };
        }

        if (localTime.IsMidNight())
        {
            settings = settings with
            {
                ThermostatBathZeroTemperature = settings.ThermostatBathZeroTemperature - 4,
                ThermostatBathOneTemperature = settings.ThermostatBathOneTemperature - 4
            };
        }
        else if (localTime.IsWeekdayMidDay() || localTime.IsEvening())
        {
            settings = settings with
            {
                ThermostatBathZeroTemperature = settings.ThermostatBathZeroTemperature - 2,
                ThermostatBathOneTemperature = settings.ThermostatBathOneTemperature - 2
            };
        }

        if (IsPeriodicIncreaseWindow(localTime))
        {
            settings = settings with { OpMode = OpMode.Auto };
        }
        else if (ShouldForceManualOpMode(prices, profile, at))
        {
            settings = settings with { OpMode = OpMode.Manual };
        }

        return settings;
    }

    private static bool IsPeriodicIncreaseWindow(DateTime localTime)
    {
        var startDate = new DateOnly(2026, 11, 20);
        var daysSinceStart = DateOnly.FromDateTime(localTime).DayNumber - startDate.DayNumber;

        return daysSinceStart >= 0 &&
            daysSinceStart % 14 == 0 &&
            localTime.TimeOfDay < TimeSpan.FromHours(2);
    }

    private static bool ShouldForceManualOpMode(
        IReadOnlyList<EnergyPriceDetails> prices,
        DeviceSettingsProfile profile,
        DateTimeOffset at)
    {
        var currentPrice = PriceLevelGenerator.GetForSpecificDate(at, prices);

        if (currentPrice.StartsAt.DateTime.IsWeekdayDayTime() ||
            currentPrice.PriceTotal > profile.OpModes.MaxPriceAutoMode)
        {
            return true;
        }

        var offPeakPrices = prices.Where(p => !p.StartsAt.DateTime.IsWeekdayDayTime()).ToList();
        var lowestSixthThreshold = offPeakPrices
            .OrderBy(p => p.PriceTotal)
            .ElementAtOrDefault(offPeakPrices.Count / 6);

        return lowestSixthThreshold is null || currentPrice.PriceTotal > lowestSixthThreshold.PriceTotal;
    }
}
