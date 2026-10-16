using MyHome.Core.Services;

namespace MyHome.ApiService.Endpoints;

public static class AutomationPreviewEndpoints
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/automation/preview", (HouseAutomationService service, CancellationToken cancellationToken) =>
            service.GetPreviewAsync(cancellationToken: cancellationToken));
    }
}
