using Microsoft.AspNetCore.Components;
using MudBlazor;
using MyHome.Core.Models.EnergySupplier.Enums;
using MyHome.Web.ExternalClients;

namespace MyHome.Web.Components.Pages;
public partial class EnergyLists
{
    [Inject]
    private EnergySupplierClient EnergySupplierClient { get; set; } = default!;

    private IEnumerable<EnergyListItem> _prices = [];
    private bool _loading;

    protected override async Task OnInitializedAsync()
    {
        _loading = true;

        try
        {
            var pricesTask = EnergySupplierClient.GetEnergyPricesAsync();
            var waitTask = Task.Delay(500);

            await Task.WhenAll(pricesTask, waitTask);

            var prices = await pricesTask;

            _prices = prices.Select(p => new EnergyListItem()
            {
                Consumption = p.Consumption,
                Cost = p.Cost,
                LevelExternal = p.PriceDetails.LevelExternal,
                LevelInternal = p.PriceDetails.LevelInternal,
                PriceTotal = p.PriceDetails.PriceTotal,
                StartsAt = p.PriceDetails.StartsAt,
            }).ToList();
        }
        finally
        {
            _loading = false;
        }
    }
}

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