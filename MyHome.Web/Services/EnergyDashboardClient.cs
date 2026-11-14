using MyHome.Core.Models.EnergySupplier;

namespace MyHome.Web.Services;

public sealed class EnergyDashboardClient(HttpClient httpClient)
{
    public async Task<IReadOnlyList<EnergyConsumptionEntry>> GetPricesAsync(CancellationToken cancellationToken = default) =>
        await httpClient.GetFromJsonAsync<List<EnergyConsumptionEntry>>(
            "energysupplier/energyprice", cancellationToken) ??
        throw new InvalidOperationException("The energy price response was empty.");

    public async Task<IReadOnlyList<EnergyConsumptionHour>> GetConsumptionAsync(DateOnly day, CancellationToken cancellationToken = default) =>
        await httpClient.GetFromJsonAsync<List<EnergyConsumptionHour>>(
            $"energysupplier/energyconsumption/{day:yyyy-MM-dd}", cancellationToken) ??
        throw new InvalidOperationException("The energy consumption response was empty.");
}
