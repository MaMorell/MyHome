using MyHome.Core.Models.EnergySupplier.Enums;
using MyHome.Core.Models.Entities.Profiles;
using MyHome.Core.Models.Integrations.HeatPump;
using MyHome.Core.Models.PriceCalculations;
using MyHome.Core.PriceCalculations;

namespace MyHome.Core.Tests;

public class AutomationPreviewTests
{
    private static readonly DateTimeOffset MondayMidday = new(2026, 1, 5, 11, 1, 0, TimeSpan.FromHours(1));

    [Theory]
    [InlineData(2026, 11, 6, 0, 0, 0, 1, OpMode.Manual)]
    [InlineData(2026, 11, 19, 23, 59, 59, 1, OpMode.Manual)]
    [InlineData(2026, 11, 20, 0, 0, 0, 1, OpMode.Auto)]
    [InlineData(2026, 11, 20, 1, 59, 59, 1, OpMode.Auto)]
    [InlineData(2026, 11, 20, 2, 0, 0, 1, OpMode.Manual)]
    [InlineData(2026, 11, 21, 1, 0, 0, 1, OpMode.Manual)]
    [InlineData(2026, 11, 27, 1, 0, 0, 1, OpMode.Manual)]
    [InlineData(2026, 12, 4, 0, 0, 0, 1, OpMode.Auto)]
    [InlineData(2026, 12, 18, 1, 0, 0, 1, OpMode.Auto)]
    [InlineData(2027, 1, 1, 1, 0, 0, 1, OpMode.Auto)]
    [InlineData(2027, 7, 2, 0, 0, 0, 2, OpMode.Auto)]
    [InlineData(2027, 7, 2, 2, 0, 0, 2, OpMode.Manual)]
    public void PeriodicIncreaseWindowOverridesPriceRulesEveryFourteenDays(
        int year, int month, int day, int hour, int minute, int second, int offsetHours, OpMode expectedMode)
    {
        var time = new DateTimeOffset(year, month, day, hour, minute, second, TimeSpan.FromHours(offsetHours));
        var price = new EnergyPriceDetails
        {
            StartsAt = time,
            PriceTotal = 10m,
            LevelInternal = EnergyPriceLevel.VeryExpensive
        };

        var targets = DeviceTargetsPlanner.Create(price, [price], new DeviceSettingsProfile(), time);

        Assert.Equal(expectedMode, targets.OpMode);
    }

    [Theory]
    [InlineData(EnergyPriceLevel.VeryCheap, 2, 26, 28, 14, 23, 63)]
    [InlineData(EnergyPriceLevel.Cheap, 1, 25, 27, 13, 22, 64)]
    [InlineData(EnergyPriceLevel.Normal, 0, 24, 26, 12, 21, 65)]
    [InlineData(EnergyPriceLevel.Expensive, -2, 22, 24, 10, 19, 67)]
    [InlineData(EnergyPriceLevel.VeryExpensive, -4, 20, 22, 8, 17, 69)]
    public void PriceLevelsChangeAllNumericDeviceTargets(
        EnergyPriceLevel level, int offset, int bathZero, int bathOne, int llvpGree, int llvpHero, int humidity)
    {
        var time = new DateTimeOffset(2026, 1, 4, 5, 0, 0, TimeSpan.FromHours(1));
        var price = new EnergyPriceDetails { StartsAt = time, PriceTotal = 0.1m, LevelInternal = level };

        var targets = DeviceTargetsPlanner.Create(price, [price], new DeviceSettingsProfile(), time);

        Assert.Equal(offset, targets.HeatOffset);
        Assert.Equal(bathZero, targets.ThermostatBathZeroTemperature);
        Assert.Equal(bathOne, targets.ThermostatBathOneTemperature);
        Assert.Equal(llvpGree, targets.LlvpGreeTemperature);
        Assert.Equal(llvpHero, targets.LlvpHeroTemperature);
        Assert.Equal(humidity, targets.DehumidifierTargetHumidity);
    }

    [Fact]
    public void TimeRulesModifyComfortThermostatsAndOperationMode()
    {
        var price = new EnergyPriceDetails
        {
            StartsAt = MondayMidday.AddMinutes(-1),
            PriceTotal = 0.1m,
            LevelInternal = EnergyPriceLevel.VeryCheap
        };

        var targets = DeviceTargetsPlanner.Create(price, [price], new DeviceSettingsProfile(), MondayMidday);

        Assert.Equal(ComfortMode.Economy, targets.ComfortMode);
        Assert.Equal(OpMode.Manual, targets.OpMode);
        Assert.Equal(24, targets.ThermostatBathZeroTemperature);
        Assert.Equal(26, targets.ThermostatBathOneTemperature);
    }

    [Fact]
    public void MidnightRulesLowerBathroomTargetsWithoutChangingGarageTarget()
    {
        var time = new DateTimeOffset(2026, 1, 4, 1, 0, 0, TimeSpan.FromHours(1));
        var price = new EnergyPriceDetails
        {
            StartsAt = time,
            PriceTotal = 0.1m,
            LevelInternal = EnergyPriceLevel.Cheap
        };

        var targets = DeviceTargetsPlanner.Create(price, [price], new DeviceSettingsProfile(), time);

        Assert.Equal(ComfortMode.Economy, targets.ComfortMode);
        Assert.Equal(OpMode.Auto, targets.OpMode);
        Assert.Equal(21, targets.ThermostatBathZeroTemperature);
        Assert.Equal(23, targets.ThermostatBathOneTemperature);
        Assert.Equal(13, targets.LlvpGreeTemperature);
        Assert.Equal(22, targets.LlvpHeroTemperature);
    }
}
