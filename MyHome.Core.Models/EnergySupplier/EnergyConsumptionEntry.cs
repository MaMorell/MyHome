using MyHome.Core.Models.PriceCalculations;

namespace MyHome.Core.Models.EnergySupplier;

public record EnergyConsumptionEntry
{
    public required EnergyPriceDetails PriceDetails { get; set; }
    public decimal Consumption { get; set; }
    public decimal Cost { get; set; }
}
