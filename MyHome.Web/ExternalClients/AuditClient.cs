using MyHome.Core.Repositories;

namespace MyHome.Web.ExternalClients;

public class AuditClient(HttpClient httpClient)
{
    public async Task<IEnumerable<AuditEvent>> GetAuditEventsAsync(CancellationToken cancellationToken = default)
    {
        var result = await httpClient.GetFromJsonAsync<IEnumerable<AuditEvent>>($"auditevents?count={20}", cancellationToken);

        return result ?? [];
    }
}
