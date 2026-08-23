namespace MyHome.Infrastructure.Options;

public class ThermostatTuyaOptions
{
    public const string ConfigurationSection = "ThermostatTuya";
    public required string AccessId { get; set; }
    public required string ApiSecret { get; set; }
    public required string DeviceId { get; set; }
}
