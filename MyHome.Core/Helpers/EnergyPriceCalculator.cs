using MyHome.Core.Models.EnergySupplier;
using MyHome.Core.Models.EnergySupplier.Enums;
using Tibber.Sdk;
using PriceLevel = Tibber.Sdk.PriceLevel;

namespace MyHome.Core.Helpers;

public class EnergyPriceCalculator
{
    private const decimal ExtremelyHighPrice = 3m;

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
        var i = 0;
        while (true)
        {
            var pricesFromIndex = pricesOrderedByTime.Skip(i).ToList();
            if (pricesFromIndex.Count < 8)
            {
                break;
            }

            pricesFromIndex = pricesFromIndex.Take(8).ToList();
            var priceOnIndex = pricesFromIndex.First();

            var average = pricesFromIndex.Average(p => p.Total) ?? 0;
            var veryHighThreshold = 1.4m * average;
            var highThreshold = 1.2m * average;
            var lowThreshold = 0.8m * average;
            var veryLowThreshold = 0.6m * average;

            var energyPrice = new EnergyPrice
            {
                Time = DateTime.Parse(priceOnIndex.StartsAt),
                Price = priceOnIndex.Total ?? 0,
                PriceLevel = ConvertToPriceLevel(priceOnIndex.Level),
                RelativePriceLevel = GetDayPriceLevel(veryHighThreshold, highThreshold, lowThreshold, veryLowThreshold, priceOnIndex.Total, priceOnIndex.Level),
            };
            energyPrices.Add(energyPrice);

            i++;
        }

        return energyPrices;
    }

    public static Models.EnergySupplier.Enums.PriceLevel ConvertToPriceLevel(PriceLevel? level)
    {
        var levelString = level?.ToString() ?? PriceLevel.Normal.ToString();
        return Enum.Parse<Models.EnergySupplier.Enums.PriceLevel>(levelString);
    }

    private static RelativePriceLevel GetDayPriceLevel(
        decimal veryHighThreshold,
        decimal highThreshold,
        decimal lowThreshold,
        decimal veryLowThreshold,
        decimal? price,
        PriceLevel? priceLevel)
    {
        if (price is null || priceLevel is null)
        {
            throw new ArgumentException($"Price or PriceLevel is null. Price: {price}. PriceLevel: {priceLevel}");
        }

        if (price >= veryHighThreshold && priceLevel == PriceLevel.VeryExpensive && price > ExtremelyHighPrice)
        {
            return RelativePriceLevel.Extreme;
        }
        else if (price >= veryHighThreshold && priceLevel == PriceLevel.VeryExpensive)
        {
            return RelativePriceLevel.VeryHigh;
        }
        else if (price >= highThreshold && (priceLevel == PriceLevel.Expensive || priceLevel == PriceLevel.VeryExpensive))
        {
            return RelativePriceLevel.High;
        }
        else if (price <= veryLowThreshold && priceLevel == PriceLevel.VeryCheap)
        {
            return RelativePriceLevel.VeryLow;
        }
        else if (price <= lowThreshold && (priceLevel == PriceLevel.Cheap || priceLevel == PriceLevel.VeryCheap))
        {
            return RelativePriceLevel.Low;
        }

        return RelativePriceLevel.Normal;
    }
}