using MyHome.Core.Models.EnergySupplier;

namespace MyHome.Web;

public class EnergySupplierClient(HttpClient httpClient)
{
    public async Task<IEnumerable<EnergyPrice>> GetEnergyPricesAsync(CancellationToken cancellationToken = default)
    {
        var result = await httpClient.GetFromJsonAsync<IEnumerable<EnergyPrice>>("/energyprice", cancellationToken);

        return result ?? [];
    }

    public async Task<EnergyMeasurement> GetLastEnergyMeasurementAsync(CancellationToken cancellationToken = default)
    {
        var result = await httpClient.GetFromJsonAsync<EnergyMeasurement>("/energymeasurement", cancellationToken);

        return result ?? new EnergyMeasurement();
    }
}
