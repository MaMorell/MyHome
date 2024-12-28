using MyHome.Core.Helpers;
using MyHome.Core.Interfaces;
using MyHome.Core.Models.EnergySupplier;

namespace MyHome.Core.Services;

public class EnergyPriceService(IEnergyRepository energyRepository)
{
    private readonly IEnergyRepository _energyRepository = energyRepository;

    public async Task<IEnumerable<EnergyPrice>> GetEnergyPriceAsync()
    {
        var result = new List<EnergyPrice>();

        var todaysPrices = await _energyRepository.GetTodaysEnergyPrices();
        result.AddRange(EnergyPriceCalculator.CreateEneryPrices(todaysPrices));

        if (DateTime.Now.Hour >= 14)
        {
            var tomorrowsPrices = await _energyRepository.GetTomorrowsEnergyPrices();

            result.AddRange(EnergyPriceCalculator.CreateEneryPrices(tomorrowsPrices));
        }

        await AddConsumption(result);

        return result;
    }

    private async Task AddConsumption(List<EnergyPrice> prices)
    {
        var todaysConsumption = await _energyRepository.GetTodaysConsumption();

        foreach (var price in prices)
        {
            var consumption = todaysConsumption.FirstOrDefault(c =>
            {
                return c.From.HasValue && c.From.Value == price.Time;
            });
            if (consumption is null)
            {
                continue;
            }

            price.Consumption = consumption?.Consumption;
            price.Cost = consumption?.Cost;
        }
    }
}