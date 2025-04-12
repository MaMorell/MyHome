using MyHome.Core.Models.Integrations.HeatPump;

namespace MyHome.Core.Models.Entities.Profiles;

public class DeviceSettingsProfile : IEntity
{
    public Guid Id { get; set; }
    public HeatOffsetProfile HeatOffsets { get; set; } = new HeatOffsetProfile();
    public ComfortModeProfile ComfortModes { get; set; } = new ComfortModeProfile();
    public OpModeProfile OpModes { get; set; } = new OpModeProfile();
    public RadiatorTemperatureProfile RadiatorTemperatures { get; set; } = new RadiatorTemperatureProfile();
    public FloorHeaterTemperatureProfile FloorHeaterTemperatures { get; set; } = new FloorHeaterTemperatureProfile();
}

public enum DeviceSettingsMode
{
    Baseline = 0,
    Enhanced = 1,
    Moderate = 2,
    Economic = 3,
    MaxSavings = 4,
    ExtremeSavings = 5
}

public class HeatOffsetProfile
{
    public int Baseline { get; set; } = 0;
    public int Enhanced { get; set; } = 2;
    public int Moderate { get; set; } = 1;
    public int Economic { get; set; } = -2;
    public int MaxSavings { get; set; } = -3;
    public int ExtremeSavings { get; set; } = -5;
}

public class ComfortModeProfile
{
    public ComfortMode Baseline { get; set; } = ComfortMode.Economy;
    public ComfortMode Enhanced { get; set; } = ComfortMode.Normal;
    public ComfortMode Moderate { get; set; } = ComfortMode.Normal;
    public ComfortMode Economic { get; set; } = ComfortMode.Economy;
    public ComfortMode MaxSavings { get; set; } = ComfortMode.Economy;
    public ComfortMode ExtremeSavings { get; set; } = ComfortMode.Economy;
}

public class OpModeProfile
{
    public OpMode Baseline { get; set; } = OpMode.Auto;
    public OpMode Enhanced { get; set; } = OpMode.Auto;
    public OpMode Moderate { get; set; } = OpMode.Auto;
    public OpMode Economic { get; set; } = OpMode.Manual;
    public OpMode MaxSavings { get; set; } = OpMode.Manual;
    public OpMode ExtremeSavings { get; set; } = OpMode.Manual;
}

public class RadiatorTemperatureProfile
{
    public int Baseline { get; set; } = 7;
    public int Enhanced { get; set; } = 9;
    public int Moderate { get; set; } = 8;
    public int Economic { get; set; } = 6;
    public int MaxSavings { get; set; } = 5;
    public int ExtremeSavings { get; set; } = 5;
}

public class FloorHeaterTemperatureProfile
{
    public int Baseline { get; set; } = 22;
    public int Enhanced { get; set; } = 25;
    public int Moderate { get; set; } = 24;
    public int Economic { get; set; } = 15;
    public int MaxSavings { get; set; } = 10;
    public int ExtremeSavings { get; set; } = 5;
}
