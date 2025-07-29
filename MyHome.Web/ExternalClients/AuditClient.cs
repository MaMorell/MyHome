using MyHome.Core.Models.Audit;

namespace MyHome.Web.ExternalClients;

public class AuditClient(ApiServiceClient client)
{
    public async Task<IEnumerable<AuditEvent>> GetAuditEventsAsync(CancellationToken cancellationToken = default)
    {
        var result = await client.GetFromJsonAsync<IEnumerable<AuditEvent>>($"auditevents?limit={20}", cancellationToken);

        return result ?? [];
    }
}
