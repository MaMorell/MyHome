using MyHome.Core.Extensions;
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

    public async Task<ICollection<DailyEnergyConsumption>> GetConsumptionByDays(int lastDays)
    {
        var hoursPassedToday = DateTime.Now.Hour;

        var entriesToGet = lastDays * 24 + hoursPassedToday;

        var consumption = await _energyRepository.GetConsumption(entriesToGet);

        var result = GroupConsumptionByDay(lastDays, consumption);
        return result;
    }

    public async Task<IEnumerable<EnergyConsumptionEntry>> GetFutureEnergyPrices()
    {
        var prices = await _energyPriceCalculator.CreateAsync(EnergyPriceRange.TodayAndTomorrow);
        var todaysConsumption = await _energyRepository.GetConsumption(DateTime.Now.Hour);

        return CreateEnergyConsumptionEntries(prices, todaysConsumption).ToList();
    }

    public async Task<IEnumerable<EnergyConsumptionEntry>> GetTopConsumptionThisMonth(int limit, bool onlyDuringWeekdays)
    {
        var hoursPassedThisMonth = (DateTime.Now.Day - 1) * 24 + DateTime.Now.Hour;

        var consumption = await _energyRepository.GetConsumption(hoursPassedThisMonth);

        var consumptionFiltered = onlyDuringWeekdays
            ? consumption.Where(c => c.PriceDetails.StartsAt.Date.IsWeekdayDayTime())
            : consumption;

        return consumption
            .OrderByDescending(entry => entry.Consumption)
            .Take(limit);
    }

    private static IEnumerable<EnergyConsumptionEntry> CreateEnergyConsumptionEntries(IEnumerable<EnergyPriceDetails> prices, IEnumerable<EnergyConsumptionEntry> consumptions)
    {
        foreach (var priceDetails in prices)
        {
            var consumption = consumptions.FirstOrDefault(c => c.PriceDetails?.StartsAt == priceDetails.StartsAt);

            yield return new EnergyConsumptionEntry()
            {
                PriceDetails = priceDetails,
                Consumption = consumption?.Consumption ?? 0,
                Cost = consumption?.Cost ?? 0
            };
        }
    }

    private static List<DailyEnergyConsumption> GroupConsumptionByDay(int lastDays, ICollection<EnergyConsumptionEntry> consumption)
    {
        var result = new List<DailyEnergyConsumption>();

        var hoursPassedToday = DateTime.Now.Hour;

        var todaysConsumption = consumption.Take(hoursPassedToday);
        result.Add(new DailyEnergyConsumption
        {
            ConsumptionEntries = todaysConsumption,
            Date = DateOnly.FromDateTime(DateTime.Now)
        });

        for (int i = 0; i < lastDays; i++)
        {
            var dayConsumption = consumption.Skip(i * 24 + hoursPassedToday).Take(24);

            result.Add(new DailyEnergyConsumption
            {
                ConsumptionEntries = dayConsumption,
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(-(i + 1)))
            });
        }

        return result;
    }
}
