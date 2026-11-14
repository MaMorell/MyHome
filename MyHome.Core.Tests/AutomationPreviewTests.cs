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
    [InlineData(EnergyPriceLevel.VeryCheap, 2, 26, 28, 14, 63)]
    [InlineData(EnergyPriceLevel.Cheap, 1, 25, 27, 13, 64)]
    [InlineData(EnergyPriceLevel.Normal, 0, 24, 26, 12, 65)]
    [InlineData(EnergyPriceLevel.Expensive, -2, 22, 24, 10, 67)]
    [InlineData(EnergyPriceLevel.VeryExpensive, -4, 20, 22, 8, 69)]
    public void PriceLevelsChangeAllNumericDeviceTargets(
        EnergyPriceLevel level, int offset, int bathZero, int bathOne, int garage, int humidity)
    {
        var time = new DateTimeOffset(2026, 1, 4, 5, 0, 0, TimeSpan.FromHours(1));
        var price = new EnergyPriceDetails { StartsAt = time, PriceTotal = 0.1m, LevelInternal = level };

        var targets = DeviceTargetsPlanner.Create(price, [price], new DeviceSettingsProfile(), time);

        Assert.Equal(offset, targets.HeatOffset);
        Assert.Equal(bathZero, targets.ThermostatBathZeroTemperature);
        Assert.Equal(bathOne, targets.ThermostatBathOneTemperature);
        Assert.Equal(garage, targets.ThermostatGarageTemperature);
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
        Assert.Equal(13, targets.ThermostatGarageTemperature);
    }
}
