using MyHome.Core.Extensions;
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
        var energyPrices = await CreateAsync(EnergyPriceRange.TodayAndTomorrow);

        var dateRounded = date.RoundDownToClosestQuarter();

        var result = energyPrices.FirstOrDefault(p =>
            p.StartsAt.Date == dateRounded.Date
            && p.StartsAt.Hour == dateRounded.Hour
            && p.StartsAt.Minute == dateRounded.Minute);

        return result ?? throw new ArgumentException($"Price not found for {dateRounded:yyyy-MM-dd HH:mm}", nameof(date));
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
        const int pricesPerHour = 4; // The price resolution is 15 minutes

        if (prices.Count * pricesPerHour < profile.InternalPriceLevelRangeInHours)
        {
            return EnergyPriceLevel.Unknown;
        }

        var pricesFromRange = prices.Take(profile.InternalPriceLevelRangeInHours * pricesPerHour).ToList();

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