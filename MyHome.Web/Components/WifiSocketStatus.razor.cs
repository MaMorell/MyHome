using Microsoft.AspNetCore.Components;
using MudBlazor;
using MyHome.Core.Models.WifiSocket;
using MyHome.Web.ExternalClients;

namespace MyHome.Web.Components;

public partial class WifiSocketStatus
{
    [Inject]
    private WifiSocketClient WifiSocketClient { get; set; } = default!;

    [Parameter]
    public WifiSocketName WifiSocketname { get; set; }

    private ControllStatus? Status { get; set; }
    private DateTime DataUpdatedAt { get; set; }

    protected override async Task OnInitializedAsync() => await RefreshData();

    private async Task RefreshData()
    {
        Status = await WifiSocketClient.GetStatusAsync(WifiSocketname);
        DataUpdatedAt = DateTime.Now;
    }

    private Color GetTemperatureColor(double? temperature)
    {
        if (!temperature.HasValue) return Color.Default;

        return temperature switch
        {
            < 0 => Color.Error,
            < 5 => Color.Warning,
            < 25 => Color.Success,
            _ => Color.Default
        };
    }
}