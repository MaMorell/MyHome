using MyHome.Core.Models.Entities;

namespace MyHome.Core.Models.Audit;

public record AuditEvent(AuditAction Action, AuditTarget Target) : IEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime Timestamp { get; init; } = DateTime.Now;
    public string? NewValue { get; init; }
    public string? TargetName { get; init; }
}

