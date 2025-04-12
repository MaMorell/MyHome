using MyHome.Core.Models.EnergySupplier;
using MyHome.Core.Models.EnergySupplier.Enums;
using Tibber.Sdk;

namespace MyHome.Data.Extensions;

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
            StartsAt = DateTime.Parse(price.StartsAt),
            Level = price.Level.ToPriceLevel()
        };
    }

    public static EnergyPriceLevel ToPriceLevel(this PriceLevel? level)
    {
        var levelString = level?.ToString() ?? PriceLevel.Normal.ToString();
        return Enum.Parse<EnergyPriceLevel>(levelString);
    }

    public static ICollection<EnergyConsumptionEntry> ToEnergyConsumptionEntries(this IEnumerable<ConsumptionEntry> entries)
    {
        return entries
            .Select(entry => entry.ToEnergyConsumptionEntry())
            .ToList();
    }

    public static EnergyConsumptionEntry ToEnergyConsumptionEntry(this ConsumptionEntry entry)
    {
        return new EnergyConsumptionEntry
        {
            From = entry.From.GetValueOrDefault().Date,
            To = entry.To.GetValueOrDefault().Date,
            Price = entry.UnitPrice.GetValueOrDefault(),
            Consumption = entry.Consumption.GetValueOrDefault(),
            Cost = entry.Cost.GetValueOrDefault()
        };
    }
}
