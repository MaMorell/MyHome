using MyHome.Core.Models.Entities.Profiles;

namespace MyHome.Web.ExternalClients;

public class ProfilesClient(ApiServiceClient client)
{
    private const string Uri = $"/profiles/pricethearsholds";

    public async Task<PriceThearsholdsProfile> GetPriceThearsholds(CancellationToken cancellationToken = default)
    {
        var result = await client.GetFromJsonAsync<PriceThearsholdsProfile>(Uri, cancellationToken);

        return result ?? throw new HttpRequestException($"Failed to get {nameof(PriceThearsholdsProfile)}");
    }

    public async Task UpdatePriceThearsholds(PriceThearsholdsProfile profile, CancellationToken cancellationToken = default)
    {
        await client.PutAsJsonAsync(Uri, profile, cancellationToken);
    }
}
