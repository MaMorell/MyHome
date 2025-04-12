namespace MyHome.Data.Integrations.HeatPump.Dtos;

public class NibePoint
{
    public string ParameterId { get; set; } = string.Empty;
    public string ParameterName { get; set; } = string.Empty;
    public double Value { get; set; }
}
