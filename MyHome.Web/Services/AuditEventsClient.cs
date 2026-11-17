using MyHome.Core.Models.Audit;

namespace MyHome.Web.Services;

public sealed class AuditEventsClient(HttpClient httpClient)
{
    public async Task<IReadOnlyList<AuditEvent>> GetAsync(int limit = 20, CancellationToken cancellationToken = default) =>
        await httpClient.GetFromJsonAsync<List<AuditEvent>>(
            $"auditevents?limit={limit}", cancellationToken) ?? [];
}
