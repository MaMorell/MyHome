namespace MyHome.Core.Models.Entities.Profiles;

public class PriceThearsholdsProfile : IEntity
{
    public Guid Id { get; set; }
    public decimal VeryHigh { get; set; } = 1.6m;
    public decimal High { get; set; } = 1.3m;
    public decimal Low { get; set; } = 0.7m;
    public decimal VeryLow { get; set; } = 0.4m;
}
