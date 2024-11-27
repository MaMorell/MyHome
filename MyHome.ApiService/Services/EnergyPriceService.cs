using MyHome.Core.Helpers;
using MyHome.Core.Interfaces;
using MyHome.Core.Models;

namespace MyHome.ApiService.Services;

public class EnergyPriceService(IEnergyRepository energyRepository)
{
    private readonly IEnergyRepository _energyRepository = energyRepository;

    public async Task<IEnumerable<EnergyPrice>> GetEnergyPriceAsync()
    {
        var result = new List<EnergyPrice>();

        var todaysPrices = await _energyRepository.GetTodaysEnergyPrices();
        result.AddRange(HeatRegulator.CreateEneryPrices(todaysPrices));

        if (DateTime.Now.Hour > 14)
        {
            var tomorrowsPrices = await _energyRepository.GetTomorrowsEnergyPrices();

            result.AddRange(HeatRegulator.CreateEneryPrices(tomorrowsPrices));
        }

        return result;
    }
}