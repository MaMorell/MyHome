namespace MyHome.Data.Integrations.Thermostats.Models;

public class UpdateDeviceInput
{
    public int Id { get; set; }
    public bool? PowerOn { get; set; }
    public string? SelectedProgram { get; set; }
    public double? TemperatureSet { get; set; }
}
