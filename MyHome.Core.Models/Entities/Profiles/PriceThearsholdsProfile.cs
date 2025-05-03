namespace MyHome.Core.Models.Entities.Profiles;

public class PriceThearsholdsProfile : IEntity
{
    public Guid Id { get; set; }
    public decimal VeryHigh { get; set; } = 1.6m;
    public decimal High { get; set; } = 1.3m;
    public decimal Low { get; set; } = 0.7m;
    public decimal VeryLow { get; set; } = 0.4m;

    public string VeryHighPercentage => $"{(VeryHigh - 1) * 100:N0}%";
    public string HighPercentage => $"{(High - 1) * 100:N0}%";
    public string LowPercentage => $"{(1 - Low) * 100:N0}%";
    public string VeryLowPercentage => $"{(1 - VeryLow) * 100:N0}%";

    public decimal ExtremelyHighPrice { get; set; } = 3.0m;
    public int HoursForCalculaingRelativePriceLevel { get; set; } = 8;
}