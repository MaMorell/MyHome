using MyHome.Core.Models.EnergySupplier;

namespace MyHome.Web;

public class EnergyPriceClient(HttpClient httpClient)
{
    public async Task<IEnumerable<EnergyPrice>> GetEnergyPricesAsync(CancellationToken cancellationToken = default)
    {
        var result = await httpClient.GetFromJsonAsync<IEnumerable<EnergyPrice>>("/energyprice", cancellationToken);

        return result ?? [];
    }
}
