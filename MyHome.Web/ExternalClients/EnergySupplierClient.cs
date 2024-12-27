using MyHome.Core.Models.EnergySupplier;

namespace MyHome.Web.HttpClients;

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
