using MyHome.Core.Models.EnergySupplier.Enums;

namespace MyHome.Core.Models.EnergySupplier;

public record EnergyPrice
{
    public decimal? Total { get; set; }
    public DateTimeOffset StartsAt { get; init; }
    public EnergyPriceLevel? Level { get; set; }
}