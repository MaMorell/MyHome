using MyHome.Core.Interfaces;
using MyHome.Core.Models.EnergySupplier;
using MyHome.Core.Models.EnergySupplier.Enums;
using MyHome.Core.PriceCalculations;

namespace MyHome.Core.Services;

public class EnergySupplierService(IEnergyRepository energyRepository, EnergyPriceCalculator energyPriceCalculator)
{
    private readonly IEnergyRepository _energyRepository = energyRepository;
    private readonly EnergyPriceCalculator _energyPriceCalculator = energyPriceCalculator;

    public async Task<IEnumerable<EnergyConsumptionEntry>> GetFutureEnergyPricesAsync()
    {
        var prices = await _energyRepository.GetEnergyPrices(PriceType.All);

        var result = await _energyPriceCalculator.CreateEneryPrices(prices);

        result = await AddConsumptionToPrices(result);

        return result;
    }

    public async Task<ICollection<EnergyConsumptionEntry>> GetTopConumptionAsync(int limit, bool onlyDuringWeekdays)
    {
        return onlyDuringWeekdays
            ? await _energyRepository.GetTopConsumptionDuringWeekdays(limit)
            : await _energyRepository.GetTopConsumption(limit);
    }

    private async Task<IEnumerable<EnergyConsumptionEntry>> AddConsumptionToPrices(IEnumerable<EnergyConsumptionEntry> prices)
    {
        var todaysConsumption = await _energyRepository.GetConsumptionForToday();

        foreach (var price in prices)
        {
            var consumption = todaysConsumption.FirstOrDefault(c =>
            {
                return c.StartsAt == price.StartsAt;
            });
            if (consumption is null)
            {
                continue;
            }

            price.Consumption = consumption?.Consumption ?? 0;
            price.Cost = consumption?.Cost ?? 0;
        }

        return prices;
    }
}
