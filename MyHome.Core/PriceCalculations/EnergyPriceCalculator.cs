using MyHome.Core.Models.EnergySupplier;
using MyHome.Core.Models.EnergySupplier.Enums;
using Tibber.Sdk;
using PriceLevel = Tibber.Sdk.PriceLevel;

namespace MyHome.Core.PriceCalculations;

public class EnergyPriceCalculator
{
    private const decimal ExtremelyHighPrice = 3m;
    private const int HoursForCalculaingRelativePriceLevel = 8;

    public static EnergyPrice CreateEneryPrices(ICollection<Price> prices, int hour)
    {
        if (hour < 0 || hour > 24)
        {
            throw new ArgumentException($"{nameof(hour)} must be between 0 and 24");
        }

        return CreateEneryPrices(prices).FirstOrDefault(h => h.Time.Hour == hour)
            ?? throw new InvalidOperationException($"Hour {hour} not found");
    }

    public static IEnumerable<EnergyPrice> CreateEneryPrices(ICollection<Price> priceDtos)
    {
        var pricesOrderedByTime = priceDtos.OrderBy(p => p.StartsAt).ToList();

        var energyPrices = new List<EnergyPrice>();
        for (int i = 0; i < priceDtos.Count; i++)
        {
            var pricesFromIndex = pricesOrderedByTime.Skip(i).ToList();
            var relativePriceLevel = DetermineRelativePriceLevel(pricesFromIndex);

            var currentPrice = priceDtos.ElementAt(i);

            energyPrices.Add(new EnergyPrice
            {
                Time = DateTime.Parse(currentPrice.StartsAt),
                Price = currentPrice.Total ?? 0,
                PriceLevel = ConvertToPriceLevel(currentPrice.Level),
                RelativePriceLevel = relativePriceLevel,
            });
        }

        return energyPrices;
    }

    private static RelativePriceLevel DetermineRelativePriceLevel(List<Price> prices)
    {
        if (prices.Count < HoursForCalculaingRelativePriceLevel)
        {
            return RelativePriceLevel.Unknown;
        }

        prices = [.. prices.Take(HoursForCalculaingRelativePriceLevel)];

        var average = prices.Average(p => p.Total) ?? 0;
        var parameters = EnergyPriceThresholds.CreateFromAverage(
            average: prices.Average(p => p.Total) ?? 0,
            price: prices.First().Total,
            priceLevel: prices.First().Level
        );

        return CalculateRelativePriceLevel(parameters);
    }

    private static RelativePriceLevel CalculateRelativePriceLevel(EnergyPriceThresholds thresholds)
    {
        if (thresholds.Price is null || thresholds.PriceLevel is null)
        {
            throw new ArgumentException($"Price or PriceLevel is null. Price: {thresholds.Price}. PriceLevel: {thresholds.PriceLevel}");
        }

        if (thresholds.Price >= thresholds.VeryHighThreshold && thresholds.PriceLevel == PriceLevel.VeryExpensive && thresholds.Price > ExtremelyHighPrice)
        {
            return RelativePriceLevel.Extreme;
        }
        else if (thresholds.Price >= thresholds.VeryHighThreshold && thresholds.PriceLevel == PriceLevel.VeryExpensive)
        {
            return RelativePriceLevel.VeryHigh;
        }
        else if (thresholds.Price >= thresholds.HighThreshold && (thresholds.PriceLevel == PriceLevel.Expensive || thresholds.PriceLevel == PriceLevel.VeryExpensive))
        {
            return RelativePriceLevel.High;
        }
        else if (thresholds.Price <= thresholds.VeryLowThreshold && thresholds.PriceLevel == PriceLevel.VeryCheap)
        {
            return RelativePriceLevel.VeryLow;
        }
        else if (thresholds.Price <= thresholds.LowThreshold && (thresholds.PriceLevel == PriceLevel.Cheap || thresholds.PriceLevel == PriceLevel.VeryCheap))
        {
            return RelativePriceLevel.Low;
        }

        return RelativePriceLevel.Normal;
    }

    public static Models.EnergySupplier.Enums.PriceLevel ConvertToPriceLevel(PriceLevel? level)
    {
        var levelString = level?.ToString() ?? PriceLevel.Normal.ToString();
        return Enum.Parse<Models.EnergySupplier.Enums.PriceLevel>(levelString);
    }
}
