using MyHome.Core.Models.Integrations.WifiSocket;

namespace MyHome.Core.Interfaces;
public interface IWifiSocketsService
{
    Task<ControllStatus> GetStatus(string name);
    Task UpdateAllClients(int temperature, CancellationToken cancellationToken);
}