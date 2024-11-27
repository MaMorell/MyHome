using MyHome.Web.Dtos;
using System.Collections;

namespace MyHome.Web;

public class EnergyPriceClient(HttpClient httpClient)
{
    public async Task<IEnumerable<EnergyPriceDto>> GetEnergyPricesAsync(CancellationToken cancellationToken = default)
    {
        var result = await httpClient.GetFromJsonAsync<IEnumerable<EnergyPriceDto>>("/energyprice", cancellationToken);

        return result ?? Array.Empty<EnergyPriceDto>();
    }
}
