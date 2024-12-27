using Microsoft.AspNetCore.Components;
using MyHome.Web.HttpClients;

namespace MyHome.Web.Components;

public partial class EnergyMeter
{
    [Inject]
    private EnergySupplierClient EnergySupplierClient { get; set; } = default!;

    private decimal RealTimePowerUsage { get; set; }
    private decimal AccumulatedConsumptionLastHour { get; set; }
    private DateTime DataUpdatedAt { get; set; }

    protected override async Task OnInitializedAsync() => await RefreshData();

    private async Task OnRefresh() => await RefreshData();

    private async Task RefreshData()
    {
        var result = await EnergySupplierClient.GetLastEnergyMeasurementAsync();
        RealTimePowerUsage = result.Power;
        AccumulatedConsumptionLastHour = result.AccumulatedConsumptionLastHour;
        DataUpdatedAt = result.UpdatedAt;
    }
}