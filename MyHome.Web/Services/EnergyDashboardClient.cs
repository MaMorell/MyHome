using MyHome.Core.Models.EnergySupplier;

namespace MyHome.Web.Services;

public sealed class EnergyDashboardClient(HttpClient httpClient)
{
    public async Task<IReadOnlyList<EnergyConsumptionEntry>> GetPricesAsync(CancellationToken cancellationToken = default) =>
        await httpClient.GetFromJsonAsync<List<EnergyConsumptionEntry>>(
            "energysupplier/energyprice", cancellationToken) ??
        throw new InvalidOperationException("The energy price response was empty.");
}
