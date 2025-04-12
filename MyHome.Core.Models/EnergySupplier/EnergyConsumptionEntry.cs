using MyHome.Core.Models.EnergySupplier.Enums;

namespace MyHome.Core.Models.EnergySupplier;

public record EnergyConsumptionEntry
{
    public DateTime Time { get; init; }
    public DateTime From { get; init; }
    public DateTime To { get; init; }
    public decimal Price { get; init; }
    public EnergyPriceLevel PriceLevel { get; init; }
    public RelativePriceLevel RelativePriceLevel { get; init; } = RelativePriceLevel.Normal;
    public decimal Consumption { get; set; }
    public decimal Cost { get; set; }
}
