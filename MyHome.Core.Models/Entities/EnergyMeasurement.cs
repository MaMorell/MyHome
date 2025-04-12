namespace MyHome.Core.Models.Entities;

public record EnergyMeasurement : IEntity
{
    public Guid Id { get; set; }

    public decimal Power { get; set; }

    public decimal AccumulatedConsumptionLastHour { get; set; }

    public DateTime UpdatedAt { get; set; }

}
