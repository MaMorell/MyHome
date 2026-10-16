using MyHome.Core.Models.EnergySupplier;
using MyHome.Core.Models.PriceCalculations;
using System.Text.Json;

namespace MyHome.Core.Tests;

public class DailyEnergyConsumptionTests
{
    [Fact]
    public void EmptyDayCanBeSerializedForDashboard()
    {
        var day = new DailyEnergyConsumption { Date = new DateOnly(2026, 1, 1) };

        Assert.Equal(0, day.AverageCostBySpotPrice);
        Assert.Equal(0, day.AverageCostByDayPrice);
        Assert.Contains("\"AverageCostByDayPrice\":0", JsonSerializer.Serialize(day));
    }

    [Fact]
    public void DayWithEntriesRetainsCostCalculations()
    {
        var day = new DailyEnergyConsumption
        {
            ConsumptionEntries =
            [
                new EnergyConsumptionEntry { PriceDetails = new EnergyPriceDetails { PriceTotal = 1m }, Consumption = 2m, Cost = 2m },
                new EnergyConsumptionEntry { PriceDetails = new EnergyPriceDetails { PriceTotal = 3m }, Consumption = 1m, Cost = 3m }
            ]
        };

        Assert.Equal(5m, day.AverageCostBySpotPrice);
        Assert.Equal(6m, day.AverageCostByDayPrice);
    }
}
