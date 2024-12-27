using MyHome.Core.Models.WifiSocket;

namespace MyHome.Web.HttpClients;

public class WifiSocketClient(HttpClient httpClient)
{
    public async Task<ControllStatus> GetStatusAsync(WifiSocketName name, CancellationToken cancellationToken = default)
    {
        var result = await httpClient.GetFromJsonAsync<ControllStatus>($"wifisocket/{name}/status", cancellationToken);

        return result ?? throw new HttpRequestException($"Failed to get the Wifi socket status for {name}");
    }
}
