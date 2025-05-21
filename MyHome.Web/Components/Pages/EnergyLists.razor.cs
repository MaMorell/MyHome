using Microsoft.AspNetCore.Components;
using MyHome.Web.ExternalClients;
using MyHome.Web.Models;

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
