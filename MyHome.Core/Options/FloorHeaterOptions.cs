namespace MyHome.Core.Repositories.FloorHeating;

public class FloorHeaterOptions
{
    public const string ConfigurationSection = "FloorHeater";
    public required string AccessId { get; set; }
    public required string ApiSecret { get; set; }
    public required string DeviceId { get; set; }
}
    