namespace MyHome.Core.Models.Entities.Profiles;

public class PriceThearsholdsProfile : IEntity
{
    public Guid Id { get; set; }
    public decimal VeryExpensive { get; set; } = 1.6m;
    public decimal Expensive { get; set; } = 1.3m;
    public decimal Cheap { get; set; } = 0.7m;
    public decimal VeryCheap { get; set; } = 0.4m;
    public decimal Extreme { get; set; } = 3.0m;
    public int InternalPriceLevelRangeInHours { get; set; } = 8;

    public string VeryHighPercentage => $"{(VeryExpensive - 1) * 100:N0}%";
    public string HighPercentage => $"{(Expensive - 1) * 100:N0}%";
    public string LowPercentage => $"{(1 - Cheap) * 100:N0}%";
    public string VeryLowPercentage => $"{(1 - VeryCheap) * 100:N0}%";

}