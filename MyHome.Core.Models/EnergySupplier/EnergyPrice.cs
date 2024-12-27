namespace MyHome.Core.Models.EnergySupplier;

public record EnergyPrice
{
    public DateTime Time { get; init; }
    public decimal Price { get; init; }
    public PriceLevel PriceLevel { get; init; }
    public RelativePriceLevel RelativePriceLevel { get; init; } = RelativePriceLevel.Normal;
    public decimal? Consumption { get; set; }
    public decimal? Cost { get; set; }
}