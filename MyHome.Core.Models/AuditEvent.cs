using MyHome.Core.Models;

namespace MyHome.Core.Repositories;

public record AuditEvent(AuditAction Action, AuditTarget Target) : IEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime Timestamp { get; init; } = DateTime.Now;
    public object? NewValue { get; init; }
    public string? TargetName { get; init; }
}

