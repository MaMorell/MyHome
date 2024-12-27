using Microsoft.Extensions.Logging;
using MyHome.Core.Models.WifiSocket;

namespace MyHome.Core.Repositories.WifiSocket;

public class WifiSocketsService(
    IEnumerable<WifiSocketClient> clients,
    ILogger<WifiSocketsService> logger)
{
    private readonly IEnumerable<WifiSocketClient> _wifiSocketClients = clients ?? throw new ArgumentNullException(nameof(clients));
    private readonly ILogger<WifiSocketsService> _logger = logger;

    public async Task UpdateAllClients(int temperature, CancellationToken cancellationToken)
    {
        var tasks = new List<Task>();

        foreach (var client in _wifiSocketClients)
        {
            tasks.Add(UpdateWifiSocketSafely(client, temperature, cancellationToken));
        }

        await Task.WhenAll(tasks);
    }

    public async Task<ControllStatus> GetStatus(WifiSocketName name)
    {
        var client = _wifiSocketClients.FirstOrDefault(c => c.Name == name);
        if (client == null)
        {
            throw new ArgumentException($"No client found for wifi socket {name}", nameof(name));
        }

        return await client.GetStatus();
    }

    private async Task UpdateWifiSocketSafely(WifiSocketClient client, int temperature, CancellationToken cancellationToken)
    {
        try
        {
            var success = await client.UpdateHeat(temperature, cancellationToken);
            if (!success)
            {
                _logger.LogWarning("Failed to update temperature for radiator {RadiatorName}", client.Name);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating radiator {RadiatorName}", client.Name);
        }
    }
}
