using MyHome.Core.Models.Entities;

namespace MyHome.Core.Models.Audit;

public record AuditEvent : IEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime Timestamp { get; init; } = DateTime.Now;
    public required string Description { get; init; }
    public required object NewValue { get; init; }
    public object? OldValue { get; init; }

}

