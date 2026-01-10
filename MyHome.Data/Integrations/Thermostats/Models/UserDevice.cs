namespace MyHome.Data.Integrations.Thermostats.Models;

public class UserDeviceResponse
{
    public UserDevice? Result { get; set; }
    public bool Success { get; set; }
}

public class UserDevice
{
    public int Id { get; set; }
    public string? DisplayName { get; set; }
    public bool PowerOn { get; set; }
    public string? SelectedProgram { get; set; }
    public string? ProgramState { get; set; }
    public double TemperatureSet { get; set; }
    public double TemperatureFloor { get; set; }
    public double TemperatureRoom { get; set; }
    public double TemperatureFloorDecimals { get; set; }
    public double TemperatureRoomDecimals { get; set; }
    public bool RelayOn { get; set; }
    public int MinutesToTarget { get; set; }
    public bool RemoteInput { get; set; }
    public bool HasError { get; set; }
    public string? ErrorMessage { get; set; }
    public int TodaysOnMinutes { get; set; }
    public int InstalledEffect { get; set; }
    public UserBuilding? Building { get; set; }
    public string? SensorApplication { get; set; }
}
