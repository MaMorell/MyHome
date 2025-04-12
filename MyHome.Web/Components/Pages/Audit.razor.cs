using Microsoft.AspNetCore.Components;
using MyHome.Core.Models.Audit;
using MyHome.Web.ExternalClients;

namespace MyHome.Web.Components.Pages;
public partial class Audit
{
    [Inject]
    private AuditClient Client { get; set; } = default!;

    private IEnumerable<AuditPresentation> events = [];
    private bool _loading;

    protected override async Task OnInitializedAsync()
    {
        _loading = true;

        try
        {
            var eventsTask = Client.GetAuditEventsAsync();
            var waitTask = Task.Delay(500);

            await Task.WhenAll(eventsTask, waitTask);

            events = eventsTask.Result.Select(e => new AuditPresentation(e));
        }
        finally
        {
            _loading = false;
        }
    }
}

public class AuditPresentation(AuditEvent auditEvent)
{
    private static string TranslateAction(AuditAction action) => action switch
    {
        AuditAction.Update => "Uppdaterad",
        _ => action.ToString(),
    };

    private static string TranslateTarget(AuditEvent auditEvent) => auditEvent.Target switch
    {
        AuditTarget.HeatPump => "NIBE värmepump",
        AuditTarget.WifiSocket => $"Element - {auditEvent.TargetName}",
        _ => auditEvent.Target.ToString(),
    };

    public string Action { get; } = TranslateAction(auditEvent.Action);
    public string Target { get; } = TranslateTarget(auditEvent);
    public DateTime Timestamp { get; } = auditEvent.Timestamp;
    public object? NewValue { get; } = auditEvent.NewValue;

}
