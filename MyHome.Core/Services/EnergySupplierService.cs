using MyHome.Core.Interfaces;
using MyHome.Core.Models.EnergySupplier;
using MyHome.Core.PriceCalculations;

namespace MyHome.Core.Services;

public class EnergySupplierService(IEnergyRepository energyRepository)
{
    private readonly IEnergyRepository _energyRepository = energyRepository;

    public async Task<IEnumerable<EnergyPrice>> GetFutureEnergyPricesAsync()
    {
        var prices = await _energyRepository.GetAllAvailableEnergyPrices();

        var result = EnergyPriceCalculator.CreateEneryPrices(prices);

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

    private async Task AddConsumptionToPrices(IEnumerable<EnergyPrice> prices)
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