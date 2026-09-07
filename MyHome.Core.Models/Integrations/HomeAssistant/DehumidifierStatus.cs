namespace MyHome.Core.Models.Integrations.HomeAssistant;

public record DehumidifierStatus
{
    public required bool IsOn { get; init; }
    public required string Mode { get; init; }
    public required int TargetHumidityPercent { get; init; }
    public int? CurrentHumidityPercent { get; init; }
}
