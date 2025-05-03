using MyHome.Core.Interfaces;
using MyHome.Core.Models.EnergySupplier;
using MyHome.Core.Models.EnergySupplier.Enums;
using MyHome.Core.Models.Entities.Constants;
using MyHome.Core.Models.Entities.Profiles;
using MyHome.Core.Models.PriceCalculations;

namespace MyHome.Core.PriceCalculations;

public class EnergyPriceCalculator
{
    private readonly IRepository<PriceThearsholdsProfile> _priceThearsholdsService;

    public EnergyPriceCalculator(IRepository<PriceThearsholdsProfile> priceThearsholdsService)
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
        var priceThearsholds = await _priceThearsholdsService.GetByIdAsync(EntityIdConstants.PriceThearsholdsId);

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

            var currentPrice = prices.ElementAt(i);

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
        if (prices.Count < priceThearsholds.HoursForCalculaingRelativePriceLevel)
        {
            return RelativePriceLevel.Unknown;
        }

        prices = [.. prices.Take(priceThearsholds.HoursForCalculaingRelativePriceLevel)];

        var average = prices.Average(p => p.Total) ?? 0;

        var parameters = EnergyPriceThresholds.CreateFromAverage(
            average,
            prices.First().Total,
            prices.First().Level,
            priceThearsholds
        );

        return CalculateRelativePriceLevel(parameters, priceThearsholds);
    }

    private static RelativePriceLevel CalculateRelativePriceLevel(EnergyPriceThresholds thresholds, PriceThearsholdsProfile priceThearsholds)
    {
        if (thresholds.Price is null || thresholds.PriceLevel is null)
        {
            throw new ArgumentException($"Price or PriceLevel is null. Price: {thresholds.Price}. PriceLevel: {thresholds.PriceLevel}");
        }

        if (thresholds.Price >= thresholds.VeryHighThreshold && thresholds.PriceLevel == EnergyPriceLevel.VeryExpensive && thresholds.Price > priceThearsholds.ExtremelyHighPrice)
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
