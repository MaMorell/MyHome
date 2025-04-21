using MyHome.Core.Models.EnergySupplier;
using MyHome.Core.Models.EnergySupplier.Enums;
using MyHome.Core.Models.Entities.Profiles;
using MyHome.Core.Models.PriceCalculations;
using MyHome.Core.Services;

namespace MyHome.Core.PriceCalculations;

public class EnergyPriceCalculator
{
    private const decimal ExtremelyHighPrice = 3m;
    private const int HoursForCalculaingRelativePriceLevel = 8;
    private readonly PriceThearsholdsService _priceThearsholdsService;

    public EnergyPriceCalculator(PriceThearsholdsService priceThearsholdsService)
    {
        _priceThearsholdsService = priceThearsholdsService;
    }

    public async Task<EnergyConsumptionEntry> CreateEneryPrices(ICollection<EnergyPrice> prices, DateTime date)
    {
        var eneryPrices = await CreateEneryPrices(prices);

        return eneryPrices.FirstOrDefault(p =>
            p.StartsAt.Date == date.Date &&
            p.StartsAt.Hour == date.Hour)
            ?? throw new InvalidOperationException($"Hour {date.Hour} not found for date {date.Date:yyyy-MM-dd}");
    }

    public async Task<IEnumerable<EnergyConsumptionEntry>> CreateEneryPrices(ICollection<EnergyPrice> prices)
    {
        var priceThearsholds = await _priceThearsholdsService.GetThearsholdsProfile();

        var pricesOrderedByTime = prices.OrderBy(p => p.StartsAt).ToList();

        var energyPrices = new List<EnergyConsumptionEntry>();
        for (int i = 0; i < prices.Count; i++)
        {
            var pricesFromIndex = pricesOrderedByTime.Skip(i).ToList();
            var relativePriceLevel = DetermineRelativePriceLevel(pricesFromIndex, priceThearsholds);

            if (relativePriceLevel == RelativePriceLevel.Unknown)
            {
                continue;
            }

            EnergyPrice currentPrice = prices.ElementAt(i);

            energyPrices.Add(new EnergyConsumptionEntry
            {
                StartsAt = currentPrice.StartsAt,
                Price = currentPrice.Total ?? 0,
                PriceLevel = currentPrice.Level ?? EnergyPriceLevel.Normal,
                RelativePriceLevel = relativePriceLevel,
            });
        }

        return energyPrices;
    }

    private static RelativePriceLevel DetermineRelativePriceLevel(List<EnergyPrice> prices, PriceThearsholdsProfile priceThearsholds)
    {
        if (prices.Count < HoursForCalculaingRelativePriceLevel)
        {
            return RelativePriceLevel.Unknown;
        }

        prices = [.. prices.Take(HoursForCalculaingRelativePriceLevel)];

        var average = prices.Average(p => p.Total) ?? 0;

        var parameters = EnergyPriceThresholds.CreateFromAverage(
            average,
            prices.First().Total,
            prices.First().Level,
            priceThearsholds
        );

        return CalculateRelativePriceLevel(parameters);
    }

    private static RelativePriceLevel CalculateRelativePriceLevel(EnergyPriceThresholds thresholds)
    {
        if (thresholds.Price is null || thresholds.PriceLevel is null)
        {
            throw new ArgumentException($"Price or PriceLevel is null. Price: {thresholds.Price}. PriceLevel: {thresholds.PriceLevel}");
        }

        if (thresholds.Price >= thresholds.VeryHighThreshold && thresholds.PriceLevel == EnergyPriceLevel.VeryExpensive && thresholds.Price > ExtremelyHighPrice)
        {
            return RelativePriceLevel.Extreme;
        }
        else if (thresholds.Price >= thresholds.VeryHighThreshold && thresholds.PriceLevel == EnergyPriceLevel.VeryExpensive)
        {
            return RelativePriceLevel.VeryHigh;
        }
        else if (thresholds.Price >= thresholds.HighThreshold && (thresholds.PriceLevel == EnergyPriceLevel.Expensive || thresholds.PriceLevel == EnergyPriceLevel.VeryExpensive))
        {
            return RelativePriceLevel.High;
        }
        else if (thresholds.Price <= thresholds.VeryLowThreshold && thresholds.PriceLevel == EnergyPriceLevel.VeryCheap)
        {
            return RelativePriceLevel.VeryLow;
        }
        else if (thresholds.Price <= thresholds.LowThreshold && (thresholds.PriceLevel == EnergyPriceLevel.Cheap || thresholds.PriceLevel == EnergyPriceLevel.VeryCheap))
        {
            return RelativePriceLevel.Low;
        }

        return RelativePriceLevel.Normal;
    }
}
