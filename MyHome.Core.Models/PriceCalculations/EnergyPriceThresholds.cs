using MyHome.Core.Models.EnergySupplier;
using MyHome.Core.Models.Entities.Profiles;

namespace MyHome.Core.Models.PriceCalculations;

public class PriceThresholds
{
    private PriceThresholds(
        decimal veryHighThreshold,
        decimal highThreshold,
        decimal lowThreshold,
        decimal veryLowThreshold,
        decimal extremeThreshold)
    {
        VeryHighThreshold = veryHighThreshold;
        HighThreshold = highThreshold;
        LowThreshold = lowThreshold;
        VeryLowThreshold = veryLowThreshold;
        ExtremeThreshold = extremeThreshold;
    }

    public decimal ExtremeThreshold { get; }
    public decimal VeryHighThreshold { get; }
    public decimal HighThreshold { get; }
    public decimal LowThreshold { get; }
    public decimal VeryLowThreshold { get; }

    public static PriceThresholds Create(IEnumerable<EnergyPrice> prices, PriceThearsholdsProfile profile)
    {
        var average = prices.Average(p => p.Total) ?? 0;

        return new PriceThresholds(
            profile.VeryExpensive * average,
            profile.Expensive * average,
            profile.Cheap * average,
            profile.VeryCheap * average,
            profile.Extreme
        );
    }
}
