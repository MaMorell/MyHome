namespace MyHome.Core.Models.Entities.Profiles;

public class PriceThearsholdsProfile : IEntity
{
    public Guid Id { get; set; }
    public decimal VeryHigh { get; } = 1.6m;
    public decimal High { get; } = 1.3m;
    public decimal Low { get; } = 0.7m;
    public decimal VeryLow { get; } = 0.4m;
}
