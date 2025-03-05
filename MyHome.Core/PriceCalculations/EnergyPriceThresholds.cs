using PriceLevel = Tibber.Sdk.PriceLevel;

namespace MyHome.Core.PriceCalculations;

public class EnergyPriceThresholds(
    decimal veryHighThreshold,
    decimal highThreshold,
    decimal lowThreshold,
    decimal veryLowThreshold,
    decimal? price,
    PriceLevel? priceLevel)
{
    public decimal VeryHighThreshold { get; } = veryHighThreshold;
    public decimal HighThreshold { get; } = highThreshold;
    public decimal LowThreshold { get; } = lowThreshold;
    public decimal VeryLowThreshold { get; } = veryLowThreshold;
    public decimal? Price { get; } = price;
    public PriceLevel? PriceLevel { get; } = priceLevel;

    public static EnergyPriceThresholds CreateFromAverage(decimal average, decimal? price, PriceLevel? priceLevel)
    {
        return new EnergyPriceThresholds(
            veryHighThreshold: 1.6m * average,
            highThreshold: 1.3m * average,
            lowThreshold: 0.7m * average,
            veryLowThreshold: 0.4m * average,
            price: price,
            priceLevel: priceLevel
        );
    }
}