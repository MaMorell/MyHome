using MyHome.Core.Models.EnergySupplier.Enums;

namespace MyHome.Core.Models.EnergySupplier;

public record EnergyConsumptionEntry
{
    public DateTimeOffset StartsAt { get; set; }
    public decimal Price { get; set; }
    public EnergyPriceLevel PriceLevel { get; set; }
    public RelativePriceLevel RelativePriceLevel { get; set; } = RelativePriceLevel.Normal;
    public decimal Consumption { get; set; }
    public decimal Cost { get; set; }
}
