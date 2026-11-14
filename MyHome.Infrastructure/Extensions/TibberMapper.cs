using MyHome.Core.Models.EnergySupplier;
using MyHome.Core.Models.EnergySupplier.Enums;
using MyHome.Core.Models.PriceCalculations;
using Tibber.Sdk;

namespace MyHome.Infrastructure.Extensions;

public static class TibberMapper
{
    public static ICollection<EnergyPrice> ToEnergyPrices(this IEnumerable<Price> prices)
    {
        return prices
            .Select(price => price.ToEnergyPrice())
            .ToList();
    }

    public static EnergyPrice ToEnergyPrice(this Price price)
    {
        return new EnergyPrice
        {
            Total = price.Total,
            StartsAt = DateTimeOffset.Parse(price.StartsAt),
            Level = price.Level.ToPriceLevel()
        };
    }

    public static EnergyPriceLevel ToPriceLevel(this PriceLevel? level)
    {
        var levelString = level?.ToString() ?? PriceLevel.Normal.ToString();
        return Enum.Parse<EnergyPriceLevel>(levelString);
    }

    public static ICollection<EnergyConsumptionHour> ToEnergyConsumptionHours(this IEnumerable<ConsumptionEntry> entries)
    {
        return entries
            .Where(entry => entry.From.HasValue)
            .Select(entry => new EnergyConsumptionHour
            {
                StartsAt = entry.From!.Value,
                ConsumptionKWh = entry.Consumption ?? 0
            })
            .ToList();
    }
}
