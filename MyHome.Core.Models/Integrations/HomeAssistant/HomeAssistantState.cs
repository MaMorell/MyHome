using System.Text.Json;

namespace MyHome.Core.Models.Integrations.HomeAssistant;

/// <summary>
/// Mirrors the response shape of Home Assistant's REST API
/// <c>GET /api/states/{entity_id}</c> endpoint.
/// </summary>
public record HomeAssistantState
{
    public required string EntityId { get; init; }
    public required string State { get; init; }
    public Dictionary<string, JsonElement> Attributes { get; init; } = [];
    public DateTimeOffset LastChanged { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
