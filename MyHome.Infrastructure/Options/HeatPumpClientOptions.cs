namespace MyHome.Infrastructure.Options;

public class HeatPumpClientOptions
{
    public required string MqttHost { get; init; }
    public int MqttPort { get; init; } = 1883;
}