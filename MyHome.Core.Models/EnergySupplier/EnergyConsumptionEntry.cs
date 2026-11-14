using MyHome.Core.Models.PriceCalculations;

namespace MyHome.Core.Models.EnergySupplier;

public record EnergyConsumptionEntry
{
    public required EnergyPriceDetails PriceDetails { get; set; }
}
