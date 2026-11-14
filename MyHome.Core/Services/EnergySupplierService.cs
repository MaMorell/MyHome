using MyHome.Core.Interfaces;
using MyHome.Core.Models.EnergySupplier;
using MyHome.Core.Models.EnergySupplier.Enums;
using MyHome.Core.PriceCalculations;

namespace MyHome.Core.Services;

public class EnergySupplierService(
    IEnergySupplierRepository energyRepository,
    PriceLevelGenerator energyPriceCalculator)
{
    private readonly IEnergySupplierRepository _energyRepository = energyRepository;
    private readonly PriceLevelGenerator _energyPriceCalculator = energyPriceCalculator;

    public async Task<IEnumerable<EnergyConsumptionEntry>> GetFutureEnergyPrices()
    {
        var prices = await _energyPriceCalculator.CreateAsync(EnergyPriceRange.TodayAndTomorrow);

        return prices
            .Select(price => new EnergyConsumptionEntry { PriceDetails = price })
            .ToList();
    }

    public async Task<IReadOnlyList<EnergyConsumptionHour>> GetHourlyConsumptionAsync(DateOnly day) =>
        (await _energyRepository.GetEnergyConsumption(day)).ToList();
}
