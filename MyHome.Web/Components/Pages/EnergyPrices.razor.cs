using Microsoft.AspNetCore.Components;
using MyHome.Core.Models.EnergySupplier;
using MyHome.Web.HttpClients;

namespace MyHome.Web.Components.Pages;
public partial class EnergyPrices
{
    [Inject]
    private EnergySupplierClient EnergySupplierClient { get; set; } = default!;

    private IEnumerable<EnergyPrice> prices = [];
    private bool _loading;

    protected override async Task OnInitializedAsync()
    {
        _loading = true;

        try
        {
            var pricesTask = EnergySupplierClient.GetEnergyPricesAsync();
            var waitTask = Task.Delay(500);

            await Task.WhenAll(pricesTask, waitTask);

            prices = await pricesTask;
        }
        finally
        {
            _loading = false;
        }

    }
}