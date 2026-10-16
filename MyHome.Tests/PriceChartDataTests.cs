using MyHome.Core.Models.EnergySupplier;
using MyHome.Core.Models.PriceCalculations;
using MyHome.Web.Services;

namespace MyHome.Tests;

public class PriceChartDataTests
{
    [Fact]
    public void MatchesAllFourPriceIntervalsToTheReportedHour()
    {
        var start = new DateTimeOffset(2026, 10, 15, 12, 0, 0, TimeSpan.FromHours(2));
        var reported = new EnergyConsumptionEntry
        {
            PriceDetails = new EnergyPriceDetails { StartsAt = start },
            Consumption = 1.4m
        };

        foreach (var minutes in new[] { 0, 15, 30, 45 })
        {
            Assert.Same(reported, PriceChartData.ConsumptionAt([reported], start.AddMinutes(minutes)));
        }

        Assert.Null(PriceChartData.ConsumptionAt([reported], start.AddHours(1)));
    }

    [Fact]
    public void DistinguishesReportedZeroFromUnavailableHour()
    {
        var start = new DateTimeOffset(2026, 10, 15, 12, 0, 0, TimeSpan.FromHours(2));
        var reported = new EnergyConsumptionEntry
        {
            PriceDetails = new EnergyPriceDetails { StartsAt = start },
            Consumption = 0
        };

        Assert.Same(reported, PriceChartData.ConsumptionAt([reported], start.AddMinutes(30)));
        Assert.Null(PriceChartData.ConsumptionAt([], start.AddMinutes(30)));
    }

    [Fact]
    public void UsesAbsoluteTimeAcrossDaylightSavingFallback()
    {
        var first = new EnergyConsumptionEntry
        {
            PriceDetails = new EnergyPriceDetails
            {
                StartsAt = new DateTimeOffset(2026, 10, 25, 2, 0, 0, TimeSpan.FromHours(2))
            },
            Consumption = 1m
        };
        var second = new EnergyConsumptionEntry
        {
            PriceDetails = new EnergyPriceDetails
            {
                StartsAt = new DateTimeOffset(2026, 10, 25, 2, 0, 0, TimeSpan.FromHours(1))
            },
            Consumption = 2m
        };

        Assert.Same(first, PriceChartData.ConsumptionAt([first, second], first.PriceDetails.StartsAt.AddMinutes(45)));
        Assert.Same(second, PriceChartData.ConsumptionAt([first, second], second.PriceDetails.StartsAt.AddMinutes(45)));
    }
}
