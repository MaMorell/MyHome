using MyHome.Core.Models.Entities.Constants;
using MyHome.Core.Models.Entities.Profiles;

namespace MyHome.Web.ExternalClients;

public class PriceThearsholdsClient(ApiServiceClient client)
{
    private const string BaseUri = $"/profiles/price-thearsholds";

    public async Task<PriceThearsholdsProfile> GetPriceThearsholds(CancellationToken cancellationToken = default)
    {
        var uri = $"{BaseUri}/{EntityIdConstants.PriceThearsholdsId}";

        var result = await client.GetFromJsonAsync<PriceThearsholdsProfile>(uri, cancellationToken);

        return result ?? throw new HttpRequestException($"Failed to get {nameof(PriceThearsholdsProfile)}");
    }

    public async Task UpdatePriceThearsholds(PriceThearsholdsProfile profile, CancellationToken cancellationToken = default)
    {
        var uri = $"{BaseUri}/{profile.Id}";
        await client.PutAsJsonAsync(uri, profile, cancellationToken);
    }
}
