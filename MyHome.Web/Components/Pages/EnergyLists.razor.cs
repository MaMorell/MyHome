using Microsoft.AspNetCore.Components;
using MyHome.Core.Models.EnergySupplier;
using MyHome.Web.ExternalClients;

namespace MyHome.Web.Components.Pages;
public partial class EnergyLists
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