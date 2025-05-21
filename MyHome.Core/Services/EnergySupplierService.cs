using MyHome.Core.Interfaces;
using MyHome.Core.Models.EnergySupplier;
using MyHome.Core.Models.EnergySupplier.Enums;
using MyHome.Core.Models.PriceCalculations;
using MyHome.Core.PriceCalculations;

namespace MyHome.Core.Services;

public class EnergySupplierService(IEnergySupplierRepository energyRepository, PriceLevelGenerator energyPriceCalculator)
{
    private readonly IEnergySupplierRepository _energyRepository = energyRepository;
    private readonly PriceLevelGenerator _energyPriceCalculator = energyPriceCalculator;

    public async Task<IEnumerable<EnergyConsumptionEntry>> GetFutureEnergyPricesAsync()
    {
        var prices = await _energyPriceCalculator.CreateAsync(EnergyPriceRange.TodayAndTomorrow);
        var todaysConsumption = await _energyRepository.GetConsumptionForToday();

        var foo = CreateEnergyConsumptionEntries(prices, todaysConsumption).ToList();
        return foo;
    }

    public async Task<ICollection<EnergyConsumptionEntry>> GetTopConumptionAsync(int limit, bool onlyDuringWeekdays)
    {
        return onlyDuringWeekdays
            ? await _energyRepository.GetTopConsumptionDuringWeekdays(limit)
            : await _energyRepository.GetTopConsumption(limit);
    }

    private IEnumerable<EnergyConsumptionEntry> CreateEnergyConsumptionEntries(IEnumerable<EnergyPriceDetails> prices, IEnumerable<EnergyConsumptionEntry> todaysConsumption)
    {
        foreach (var priceDetails in prices)
        {
            var consumption = todaysConsumption.FirstOrDefault(c => c.PriceDetails?.StartsAt == priceDetails.StartsAt);

            yield return new EnergyConsumptionEntry()
            {
                PriceDetails = priceDetails,
                Consumption = consumption?.Consumption ?? 0,
                Cost = consumption?.Cost ?? 0
            };
        }
    }
}
