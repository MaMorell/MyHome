using MyHome.Core.Models.Entities.Constants;
using MyHome.Core.Models.Entities.Profiles;

namespace MyHome.Web.Services;

public sealed class DeviceSettingsClient(HttpClient httpClient)
{
    public async Task<DeviceSettingsProfile> GetAsync(CancellationToken cancellationToken = default) =>
        await httpClient.GetFromJsonAsync<DeviceSettingsProfile>(
            $"profiles/device-settings/{EntityIdConstants.DeviceSettingsId}", cancellationToken) ??
        throw new InvalidOperationException("The device settings profile response was empty.");

    public async Task SaveAsync(DeviceSettingsProfile profile, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PutAsJsonAsync(
            $"profiles/device-settings/{profile.Id}", profile, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
