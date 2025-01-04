using MyHome.Core.Models.EnergySupplier;
using MyHome.Core.Repositories;

namespace MyHome.Web.ExternalClients;

public class EnergySupplierClient(HttpClient httpClient)
{
    public async Task<IEnumerable<EnergyPrice>> GetEnergyPricesAsync(CancellationToken cancellationToken = default)
    {
        var result = await httpClient.GetFromJsonAsync<IEnumerable<EnergyPrice>>("energysupplier/energyprice", cancellationToken);

        return result ?? [];
    }

    public async Task<EnergyMeasurement> GetLastEnergyMeasurementAsync(CancellationToken cancellationToken = default)
    {
        var result = await httpClient.GetFromJsonAsync<EnergyMeasurement>("energysupplier/energymeasurement", cancellationToken);

        return result ?? new EnergyMeasurement();
    }
}

public class AuditClient(HttpClient httpClient)
{
    public async Task<IEnumerable<AuditEvent>> GetAuditEventsAsync(CancellationToken cancellationToken = default)
    {
        var result = await httpClient.GetFromJsonAsync<IEnumerable<AuditEvent>>($"auditevents?count={20}", cancellationToken);

        return result ?? [];
    }
}
