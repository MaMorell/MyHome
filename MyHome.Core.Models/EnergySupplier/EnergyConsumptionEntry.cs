using MyHome.Core.Models.PriceCalculations;

namespace MyHome.Core.Models.EnergySupplier;

public class DailyEnergyConsumption
{
    public DateOnly Date { get; set; }
    
    public decimal AverageCostBySpotPrice
    {
        get 
        {
            return ConsumptionEntries.Sum(c => c.Cost); 
        }
    }

    public decimal AverageCostByDayPrice
    {
        get
        {
            return ConsumptionEntries.Average(c => c.PriceDetails.PriceTotal) * ConsumptionEntries.Sum(c => c.Consumption);
        }
    }

    public IEnumerable<EnergyConsumptionEntry> ConsumptionEntries { get; set; } = [];
}

public record EnergyConsumptionEntry
{
    public required EnergyPriceDetails PriceDetails { get; set; }
    public decimal Consumption { get; set; }
    public decimal Cost { get; set; }
}
