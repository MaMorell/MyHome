using MyHome.Core.Models.EnergySupplier;

namespace MyHome.Web.Services;

public static class PriceChartData
{
    public static EnergyConsumptionEntry? ConsumptionAt(
        IEnumerable<EnergyConsumptionEntry> hourlyEntries, DateTimeOffset priceStartsAt) =>
        hourlyEntries.FirstOrDefault(entry =>
            entry.PriceDetails.StartsAt <= priceStartsAt &&
            priceStartsAt < entry.PriceDetails.StartsAt.AddHours(1));
}
