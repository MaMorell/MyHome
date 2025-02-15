using MyHome.Core.Models.EnergySupplier;

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

    public async Task<IEnumerable<EnergyConsumption>> GetHighestMonthlyConsumptionAsync(
        int limit = 5,
        bool onlyDuringWeekdays = true,
        CancellationToken cancellationToken = default)
    {
        var query = $"energysupplier/consumption/currentmonth/top?limit={limit}&onlyDuringWeekdays={onlyDuringWeekdays}";
        var result = await httpClient.GetFromJsonAsync<IEnumerable<EnergyConsumption>>(query, cancellationToken);
        return result ?? [];
    }
}