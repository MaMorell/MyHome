using MyHome.Core.Models.EnergySupplier.Enums;
using MyHome.Core.Models.Entities.Profiles;

namespace MyHome.Core.Models.PriceCalculations;

public class EnergyPriceThresholds(
    decimal veryHighThreshold,
    decimal highThreshold,
    decimal lowThreshold,
    decimal veryLowThreshold,
    decimal? price,
    EnergyPriceLevel? priceLevel)
{
    public decimal VeryHighThreshold { get; } = veryHighThreshold;
    public decimal HighThreshold { get; } = highThreshold;
    public decimal LowThreshold { get; } = lowThreshold;
    public decimal VeryLowThreshold { get; } = veryLowThreshold;
    public decimal? Price { get; } = price;
    public EnergyPriceLevel? PriceLevel { get; } = priceLevel;

    public static EnergyPriceThresholds CreateFromAverage(decimal average, decimal? price, EnergyPriceLevel? priceLevel, PriceThearsholdsProfile profile)
    {
        return new EnergyPriceThresholds(
            veryHighThreshold: profile.VeryHigh * average,
            highThreshold: profile.High * average,
            lowThreshold: profile.Low * average,
            veryLowThreshold: profile.VeryLow * average,
            price: price,
            priceLevel: priceLevel
        );
    }
}