using MyHome.Core.Models.Entities.Constants;
using MyHome.Core.Models.Entities.Profiles;

namespace MyHome.Web.ExternalClients;

public class DeviceSettingsProfileClient(ApiServiceClient client)
{
    private const string BaseUri = $"/profiles/device-settings";

    public async Task<DeviceSettingsProfile> GetPriceThearsholds(CancellationToken cancellationToken = default)
    {
        var uri = $"{BaseUri}/{EntityIdConstants.DeviceSettingsId}";

        var result = await client.GetFromJsonAsync<DeviceSettingsProfile>(uri, cancellationToken);

        return result ?? throw new HttpRequestException($"Failed to get {nameof(DeviceSettingsProfile)}");
    }

    public async Task UpdatePriceThearsholds(DeviceSettingsProfile profile, CancellationToken cancellationToken = default)
    {
        var uri = $"{BaseUri}/{profile.Id}";
        await client.PutAsJsonAsync(uri, profile, cancellationToken);
    }
}
