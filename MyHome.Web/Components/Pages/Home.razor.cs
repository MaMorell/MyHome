
using Microsoft.AspNetCore.Components.Web;

namespace MyHome.Web.Components.Pages;
public partial class Home(EnergySupplierClient energySupplierClient)
{
    private readonly EnergySupplierClient _energySupplierClient = energySupplierClient;

    private decimal RealTimePowerUsage { get; set; }
    private decimal AccumulatedConsumptionLastHour { get; set; }
    private DateTime DataUpdatedAt { get; set; }

    protected override Task OnInitializedAsync()
    {
        return RefreshData();
    }

    private async Task OnRefresh(MouseEventArgs e)
    {
        await RefreshData();
    }

    private async Task RefreshData()
    {
        var result = await _energySupplierClient.GetLastEnergyMeasurementAsync();
        RealTimePowerUsage = result.Power;
        AccumulatedConsumptionLastHour = result.AccumulatedConsumptionLastHour;
        DataUpdatedAt = result.UpdatedAt;
    }
}