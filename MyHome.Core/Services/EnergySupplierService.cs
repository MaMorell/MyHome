using MyHome.Core.Helpers;
using MyHome.Core.Interfaces;
using MyHome.Core.Models.EnergySupplier;

namespace MyHome.Core.Services;

public class EnergySupplierService(IEnergyRepository energyRepository)
{
    private readonly IEnergyRepository _energyRepository = energyRepository;

    public async Task<IEnumerable<EnergyPrice>> GetFutureEnergyPricesAsync()
    {
        var todaysPrices = await _energyRepository.GetEnergyPricesForToday();

        var result = new List<EnergyPrice>();
        result.AddRange(EnergyPriceCalculator.CreateEneryPrices(todaysPrices));

        if (DateTime.Now.Hour >= 14)
        {
            var tomorrowsPrices = await _energyRepository.GetEnergyPricesForTomorrow();

            result.AddRange(EnergyPriceCalculator.CreateEneryPrices(tomorrowsPrices));
        }

        await AddConsumptionToPrices(result);

        return result;
    }

    public async Task<IEnumerable<EnergyConsumption>> GetTopConumptionAsync(int limit, bool onlyDuringWeekdays)
    {
        var conumption = onlyDuringWeekdays
            ? await _energyRepository.GetTopConsumptionDuringWeekdays(limit)
            : await _energyRepository.GetTopConsumption(limit);

        return conumption.Select(c => new EnergyConsumption()
        {
            Consumption = c.Consumption ?? 0,
            Cost = c.Cost ?? 0,
            From = c.From ?? DateTimeOffset.MinValue,
            To = c.To ?? DateTimeOffset.MinValue,
            Price = c.UnitPrice ?? 0
        }).ToList();
    }

    private async Task AddConsumptionToPrices(List<EnergyPrice> prices)
    {
        var todaysConsumption = await _energyRepository.GetConsumptionForToday();

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

public record EnergyConsumption
{
    public DateTimeOffset From { get; init; }

    public DateTimeOffset To { get; init; }

    public decimal Price { get; init; }

    public decimal Consumption { get; init; }

    public decimal Cost { get; init; }
}