using System.Runtime.Serialization;

namespace MyHome.Core.Models.EnergySupplier.Enums;

public enum EnergyPriceLevel
{
    Unknown,
    [EnumMember(Value = "VERY_CHEAP")]
    VeryCheap,
    [EnumMember(Value = "CHEAP")]
    Cheap,
    [EnumMember(Value = "NORMAL")]
    Normal,
    [EnumMember(Value = "EXPENSIVE")]
    Expensive,
    [EnumMember(Value = "VERY_EXPENSIVE")]
    VeryExpensive,
    [EnumMember(Value = "EXTREME")]
    Extreme
}
