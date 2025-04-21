using MyHome.Core.Models.Integrations.WifiSocket;

namespace MyHome.Web.ExternalClients;

public class WifiSocketClient(ApiServiceClient client)
{
    public async Task<ControllStatus?> GetStatusAsync(WifiSocketName name, CancellationToken cancellationToken = default)
    {
        var result = await client.GetFromJsonAsync<ControllStatus>($"wifisocket/{name}/status", cancellationToken);

        return result;
    }
}
