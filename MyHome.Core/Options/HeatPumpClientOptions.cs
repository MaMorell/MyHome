namespace MyHome.Core.Options;

public class HeatPumpClientOptions
{
    public required Uri BaseAddress { get; init; }
    public required string ClientIdentifier { get; init; }
    public required string ClientSecret { get; init; }
}