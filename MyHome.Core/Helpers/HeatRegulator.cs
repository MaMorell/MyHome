using MyHome.Core.Models;
using System.Diagnostics;
using System.IO.Pipelines;
using Tibber.Sdk;

namespace MyHome.Core.Helpers;

public class HeatRegulator
{
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
        ValidatePrices(priceDtos);

        var average = priceDtos.Average(p => p.Total) ?? 0;
        var veryHighThreshold = 1.6m * average;
        var highThreshold = 1.3m * average;
        var lowThreshold = 0.7m * average;
        var veryLowThreshold = 0.4m * average;

        return priceDtos
            .OrderBy(p => p.StartsAt)
            .Select(p => new EnergyPrice
            {
                Time = DateTime.Parse(p.StartsAt),
                Price = p.Total ?? 0,
                PriceLevel = p.Level ?? PriceLevel.Normal,
                RelativePriceLevel = GetDayPriceLevel(veryHighThreshold, highThreshold, lowThreshold, veryLowThreshold, p.Total, p.Level),
            }).ToList(); ;
    }

    private static RelativePriceLevel GetDayPriceLevel(decimal veryHighThreshold, decimal highThreshold, decimal lowThreshold, decimal veryLowThreshold, decimal? price, PriceLevel? priceLevel)
    {
        if (price is null || priceLevel is null)
        {
            throw new ArgumentException($"Price or PriceLevel is null. Price: {price}. PriceLevel: {priceLevel}");
        }

        if (price >= veryHighThreshold && priceLevel == PriceLevel.VeryExpensive)
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

    private static void ValidatePrices(ICollection<Price> prices)
    {
        ArgumentException.ThrowIfNullOrEmpty(nameof(prices));

        if (prices.Count > 25 || prices.Count < 23) // Due to day light savings some days will have 25 or 23 hours
        {
            throw new ArgumentException($"Too many or too few prices in the {nameof(prices)} parameter: {prices.Count}. List of prices: {GetDatesAsString(prices)}");
        }

        for (int i = 0; i < 24; i++)
        {
            if (!prices.Any(p => DateTime.Parse(p.StartsAt).Hour == i))
            {
                throw new ArgumentException($"Missing hour {i} in list of prices. List of prices: {GetDatesAsString(prices)}");
            }
        }

        static string GetDatesAsString(ICollection<Price> prices)
        {
            return string.Join(',', prices.Select(p => p.StartsAt));
        }
    }
}