using MudBlazor;
using MyHome.Core.Models.EnergySupplier.Enums;

namespace MyHome.Web.Models;

public class EnergyListItem
{
    public decimal Consumption { get; set; }
    public decimal Cost { get; set; }
    public decimal PriceTotal { get; set; }
    public DateTimeOffset StartsAt { get; init; }
    public EnergyPriceLevel LevelExternal { get; set; }
    public EnergyPriceLevel LevelInternal { get; set; }

    public string LevelExternalColor 
    {
        get
        {
            return GetPriceLevelColor(LevelExternal);
        }
    }

    public string LevelInternalColor
    {
        get
        {
            return GetPriceLevelColor(LevelInternal);
        }
    }

    private static string GetPriceLevelColor(EnergyPriceLevel level) => level switch
    {
        EnergyPriceLevel.VeryCheap => Colors.Green.Lighten1,
        EnergyPriceLevel.Cheap => Colors.LightGreen.Lighten1,
        EnergyPriceLevel.Normal => Colors.Gray.Lighten1,
        EnergyPriceLevel.Expensive => Colors.Yellow.Lighten1,
        EnergyPriceLevel.VeryExpensive => Colors.Orange.Lighten1,
        EnergyPriceLevel.Extreme => Colors.Red.Lighten1,
        _ => Colors.Gray.Darken2,
    };
}