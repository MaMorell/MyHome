namespace MyHome.Core.Models.EnergySupplier;

public record EnergyConsumption
{
    public DateTimeOffset From { get; init; }

    public DateTimeOffset To { get; init; }

    public decimal Price { get; init; }

    public decimal Consumption { get; init; }

    public decimal Cost { get; init; }
}