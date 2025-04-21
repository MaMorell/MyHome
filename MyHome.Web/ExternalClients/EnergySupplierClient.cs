using MyHome.Core.Models.EnergySupplier;
using MyHome.Core.Models.Entities;

namespace MyHome.Web.ExternalClients;

public class EnergySupplierClient(ApiServiceClient client)
{
    public async Task<IEnumerable<EnergyConsumptionEntry>> GetEnergyPricesAsync(CancellationToken cancellationToken = default)
    {
        var result = await client.GetFromJsonAsync<IEnumerable<EnergyConsumptionEntry>>("energysupplier/energyprice", cancellationToken);
        return result ?? [];
    }

    public async Task<EnergyMeasurement> GetLastEnergyMeasurementAsync(CancellationToken cancellationToken = default)
    {
        var result = await client.GetFromJsonAsync<EnergyMeasurement>("energysupplier/energymeasurement", cancellationToken);
        return result ?? new EnergyMeasurement();
    }

    public async Task<IEnumerable<EnergyConsumptionEntry>> GetHighestMonthlyConsumptionAsync(
        int limit = 5,
        bool onlyDuringWeekdays = true,
        CancellationToken cancellationToken = default)
    {
        var query = $"energysupplier/consumption/currentmonth/top?limit={limit}&onlyDuringWeekdays={onlyDuringWeekdays}";
        var result = await client.GetFromJsonAsync<IEnumerable<EnergyConsumptionEntry>>(query, cancellationToken);
        return result ?? [];
    }
}