using MyHome.Core.Models.EnergySupplier.Enums;

namespace MyHome.Core.Models.PriceCalculations;

public record EnergyPriceDetails
{
    public decimal PriceTotal { get; init; }
    public DateTimeOffset StartsAt { get; init; }
    public EnergyPriceLevel LevelExternal { get; set; }
    public EnergyPriceLevel LevelInternal { get; set; }
}