using Microsoft.AspNetCore.Components;
using MyHome.Core.Models.Audit;
using MyHome.Web.ExternalClients;

namespace MyHome.Web.Components.Pages;
public partial class Audit
{
    [Inject]
    private AuditClient Client { get; set; } = default!;

    private IEnumerable<AuditEvent> events = [];
    private bool _loading;

    protected override async Task OnInitializedAsync()
    {
        _loading = true;

        try
        {
            var eventsTask = Client.GetAuditEventsAsync();
            var waitTask = Task.Delay(500);

            await Task.WhenAll(eventsTask, waitTask);

            events = await eventsTask;
        }
        finally
        {
            _loading = false;
        }
    }
}
