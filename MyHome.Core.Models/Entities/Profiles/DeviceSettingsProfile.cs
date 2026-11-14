using MyHome.Core.Models.Integrations.HeatPump;
using MyHome.Core.Models.Interfaces;

namespace MyHome.Core.Models.Entities.Profiles;

public class DeviceSettingsProfile : IEntity
{
    public Guid Id { get; set; }
    public int HeatOffsetBaseline { get; set; } = 0;
    public int ThermostatBathZeroTemperatureBaseline { get; set; } = 24;
    public int ThermostatBathOneTemperatureBaseline { get; set; } = 26;
    public int LlvpGreeTemperatureBaseline { get; set; } = 12;
    public int LlvpHeroTemperatureBaseline { get; set; } = 21;
    public int DehumidifierTargetHumidityBaseline { get; set; } = 65;
    public ComfortModeProfile ComfortModes { get; set; } = new ComfortModeProfile();
    public OpModeProfile OpModes { get; set; } = new OpModeProfile();
}

public class ComfortModeProfile : IDeviceProfile<ComfortMode>
{
    public ComfortMode Baseline { get; set; } = ComfortMode.Economy;
    public ComfortMode Enhanced { get; set; } = ComfortMode.Normal;
    public ComfortMode Moderate { get; set; } = ComfortMode.Normal;
    public ComfortMode Economic { get; set; } = ComfortMode.Economy;
    public ComfortMode MaxSavings { get; set; } = ComfortMode.Economy;
}

public class OpModeProfile : IDeviceProfile<OpMode>
{
    public decimal MaxPriceAutoMode { get; set; } = 0.5m;
    public decimal MaxPriceAutoModeNightTime => MaxPriceAutoMode + 1m;

    public OpMode Baseline { get; set; } = OpMode.Auto;
    public OpMode Enhanced { get; set; } = OpMode.Auto;
    public OpMode Moderate { get; set; } = OpMode.Auto;
    public OpMode Economic { get; set; } = OpMode.Manual;
    public OpMode MaxSavings { get; set; } = OpMode.Manual;
}