using MyHome.Core.Models.EnergySupplier;
using MyHome.Core.Models.PriceCalculations;

namespace MyHome.Web.Services;

public sealed class EnergyDashboardClient(HttpClient httpClient)
{
    public async Task<IReadOnlyList<EnergyConsumptionEntry>> GetPricesAsync(CancellationToken cancellationToken = default) =>
        await httpClient.GetFromJsonAsync<List<EnergyConsumptionEntry>>(
            "energysupplier/energyprice", cancellationToken) ??
        throw new InvalidOperationException("The energy price response was empty.");

    public async Task<IReadOnlyList<DailyEnergyConsumption>> GetDailyConsumptionAsync(CancellationToken cancellationToken = default) =>
        await httpClient.GetFromJsonAsync<List<DailyEnergyConsumption>>(
            "energysupplier/consumption/daily?lastDays=6", cancellationToken) ??
        throw new InvalidOperationException("The daily consumption response was empty.");

    public async Task<AutomationPreview> GetAutomationPreviewAsync(CancellationToken cancellationToken = default) =>
        await httpClient.GetFromJsonAsync<AutomationPreview>(
            "automation/preview", cancellationToken) ??
        throw new InvalidOperationException("The automation preview response was empty.");
}
