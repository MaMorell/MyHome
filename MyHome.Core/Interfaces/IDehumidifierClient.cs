using MyHome.Core.Models.Integrations.HomeAssistant;

namespace MyHome.Core.Interfaces;

/// <summary>
/// Client for controlling the "Luftavfuktare 50L" dehumidifier via Home Assistant.
/// </summary>
public interface IDehumidifierClient
{
    Task<DehumidifierStatus> GetStatusAsync(CancellationToken cancellationToken = default);
    Task SetPowerAsync(bool isOn, CancellationToken cancellationToken = default);
    Task SetTargetHumidityAsync(int targetHumidityPercent, CancellationToken cancellationToken = default);
    Task SetModeAsync(string mode, CancellationToken cancellationToken = default);
}
