namespace MyHome.Web.Dtos;

public record EnergyPriceDto
{
    public DateTime Time { get; init; }
    public decimal Price { get; init; }
    public PriceLevel PriceLevel { get; init; }
    public RelativePriceLevel RelativePriceLevel { get; init; } = RelativePriceLevel.Normal;
}