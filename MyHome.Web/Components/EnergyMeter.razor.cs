using Microsoft.AspNetCore.Components;
using MyHome.Core.Models.EnergySupplier;
using MyHome.Web.ExternalClients;

namespace MyHome.Web.Components;

public partial class EnergyMeter
{
    [Inject]
    private EnergySupplierClient EnergySupplierClient { get; set; } = default!;

    private decimal RealTimePowerUsage { get; set; }
    private decimal AccumulatedConsumptionLastHour { get; set; }
    private DateTime DataUpdatedAt { get; set; }
    public IEnumerable<EnergyConsumption> TopConsumption { get; set; }

    protected override async Task OnInitializedAsync() => await RefreshData();

    private async Task OnRefresh() => await RefreshData();

    private async Task RefreshData()
    {
        var latestMeasurement = await EnergySupplierClient.GetLastEnergyMeasurementAsync();
        RealTimePowerUsage = latestMeasurement.Power;
        AccumulatedConsumptionLastHour = latestMeasurement.AccumulatedConsumptionLastHour;
        DataUpdatedAt = latestMeasurement.UpdatedAt;

        TopConsumption = await EnergySupplierClient.GetHighestMonthlyConsumptionAsync();
    }
}