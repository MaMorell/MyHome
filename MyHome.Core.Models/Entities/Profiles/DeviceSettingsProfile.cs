using MyHome.Core.Models.Integrations.HeatPump;
using MyHome.Core.Models.Interfaces;

namespace MyHome.Core.Models.Entities.Profiles;

public class DeviceSettingsProfile : IEntity
{
    public Guid Id { get; set; }
    public HeatOffsetProfile HeatOffsets { get; set; } = new HeatOffsetProfile();
    public ComfortModeProfile ComfortModes { get; set; } = new ComfortModeProfile();
    public OpModeProfile OpModes { get; set; } = new OpModeProfile();
    public ThermostatBathZeroProfile ThermostatBathZeroTemperatures { get; set; } = new ThermostatBathZeroProfile();
    public ThermostatBathOneProfile ThermostatBathOneTemperatures { get; set; } = new ThermostatBathOneProfile();
    public ThermostatGarageProfile ThermostatGarageTemperatures { get; set; } = new ThermostatGarageProfile();
    public DehumidifierTargetHumidityProfile DehumidifierTargetHumidities { get; set; } = new DehumidifierTargetHumidityProfile();
}

public class HeatOffsetProfile : IDeviceProfile<int>
{
    public int Baseline { get; set; } = 0;
    public int Enhanced { get; set; } = 2;
    public int Moderate { get; set; } = 1;
    public int Economic { get; set; } = -2;
    public int MaxSavings { get; set; } = -3;
}

public class ThermostatBathZeroProfile : IDeviceProfile<int>
{
    public int Baseline { get; set; } = 24;
    public int Enhanced { get; set; } = 27;
    public int Moderate { get; set; } = 25;
    public int Economic { get; set; } = 20;
    public int MaxSavings { get; set; } = 15;
}

public class ThermostatBathOneProfile : IDeviceProfile<int>
{
    public int Baseline { get; set; } = 26;
    public int Enhanced { get; set; } = 30;
    public int Moderate { get; set; } = 28;
    public int Economic { get; set; } = 20;
    public int MaxSavings { get; set; } = 15;
}

public class ThermostatGarageProfile : IDeviceProfile<int>
{
    public int Baseline { get; set; } = 12;
    public int Enhanced { get; set; } = 16;
    public int Moderate { get; set; } = 14;
    public int Economic { get; set; } = 10;
    public int MaxSavings { get; set; } = 8;
}

public class DehumidifierTargetHumidityProfile : IDeviceProfile<int>
{
    public int Baseline { get; set; } = 65;
    public int Enhanced { get; set; } = 55;
    public int Moderate { get; set; } = 60;
    public int Economic { get; set; } = 70;
    public int MaxSavings { get; set; } = 75;
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