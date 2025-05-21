using MyHome.Core.Interfaces;
using MyHome.Core.Models.EnergySupplier;
using MyHome.Core.Models.EnergySupplier.Enums;
using MyHome.Core.Models.Entities.Constants;
using MyHome.Core.Models.Entities.Profiles;
using MyHome.Core.Models.PriceCalculations;

namespace MyHome.Core.PriceCalculations;

public class PriceLevelGenerator
{
    private readonly IRepository<PriceThearsholdsProfile> _priceThearsholdsRepository;
    private readonly IEnergySupplierRepository _energySupplierRepository;

    public PriceLevelGenerator(IRepository<PriceThearsholdsProfile> priceThearsholdsRepository, IEnergySupplierRepository energySupplierRepository)
    {
        _priceThearsholdsRepository = priceThearsholdsRepository;
        _energySupplierRepository = energySupplierRepository;
    }

    public async Task<EnergyPriceDetails> CreateForSpecificDateAsync(DateTime date)
    {
        var eneryPrices = await CreateAsync(EnergyPriceRange.TodayAndTomorrow);

        return eneryPrices.FirstOrDefault(p =>
            p.StartsAt.Date == date.Date &&
            p.StartsAt.Hour == date.Hour)
            ?? throw new InvalidOperationException($"Hour {date.Hour} not found for date {date.Date:yyyy-MM-dd}");
    }

    public async Task<IEnumerable<EnergyPriceDetails>> CreateAsync(EnergyPriceRange priceRange)
    {
        var profile = await _priceThearsholdsRepository.GetByIdAsync(EntityIdConstants.PriceThearsholdsId);

        var prices = await _energySupplierRepository.GetEnergyPrices(priceRange);

        var pricesOrderedByTime = prices.OrderBy(p => p.StartsAt).ToList();

        var energyPrices = new List<EnergyPriceDetails>();
        for (int i = 0; i < prices.Count; i++)
        {
            var pricesFromIndex = pricesOrderedByTime.Skip(i).ToList();
            var priceLevelInternal = CalculateInternalPriceLevel(pricesFromIndex, profile);
            var currentPrice = prices.ElementAt(i);

            energyPrices.Add(new EnergyPriceDetails
            {
                StartsAt = currentPrice.StartsAt,
                PriceTotal = currentPrice.Total ?? 0,
                LevelExternal = currentPrice.Level ?? EnergyPriceLevel.Normal,
                LevelInternal = priceLevelInternal,
            });
        }

        return energyPrices;
    }

    private static EnergyPriceLevel CalculateInternalPriceLevel(List<EnergyPrice> prices, PriceThearsholdsProfile profile)
    {
        if (prices.Count < profile.InternalPriceLevelRange)
        {
            return EnergyPriceLevel.Unknown;
        }

        var pricesFromRange = prices.Take(profile.InternalPriceLevelRange).ToList();

        var priceThresholds = PriceThresholds.Create(pricesFromRange, profile);

        return ComputeInternalPriceLevel(priceThresholds, prices.First().Level, prices.First().Total);
    }

    private static EnergyPriceLevel ComputeInternalPriceLevel(PriceThresholds thresholds, EnergyPriceLevel? priceLevelExternal, decimal? price)
    {
        if (price >= thresholds.ExtremeThreshold && priceLevelExternal == EnergyPriceLevel.VeryExpensive)
        {
            return EnergyPriceLevel.Extreme;
        }
        else if (price >= thresholds.VeryHighThreshold && priceLevelExternal == EnergyPriceLevel.VeryExpensive)
        {
            return EnergyPriceLevel.VeryExpensive;
        }
        else if (price >= thresholds.HighThreshold && (priceLevelExternal == EnergyPriceLevel.Expensive || priceLevelExternal == EnergyPriceLevel.VeryExpensive))
        {
            return EnergyPriceLevel.Expensive;
        }
        else if (price <= thresholds.VeryLowThreshold && priceLevelExternal == EnergyPriceLevel.VeryCheap)
        {
            return EnergyPriceLevel.VeryCheap;
        }
        else if (price <= thresholds.LowThreshold && (priceLevelExternal == EnergyPriceLevel.Cheap || priceLevelExternal == EnergyPriceLevel.VeryCheap))
        {
            return EnergyPriceLevel.Cheap;
        }

        return EnergyPriceLevel.Normal;
    }
}