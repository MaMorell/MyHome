using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MyHome.Core.Interfaces;
using MyHome.Core.Models.Audit;
using MyHome.Core.Models.Integrations.HeatPump;
using MyHome.Data.Integrations.HeatPump.Dtos;
using MyHome.Data.Options;
using System.Net.Http.Json;

namespace MyHome.Data.Integrations.HeatPump;

public class NibeClient(HttpClient httpClient, IRepository<AuditEvent> auditRepository, IOptions<HeatPumpClientOptions> options, IMemoryCache memoryCache) : IHeatPumpClient
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly IRepository<AuditEvent> _auditRepository = auditRepository;
    private readonly IOptions<HeatPumpClientOptions> _options = options;
    private readonly IMemoryCache _memoryCache = memoryCache;

    public async Task<ComfortMode> GetComfortMode(CancellationToken cancellationToken)
    {
        var comfortModePoint = await GetPoint(NibeParameterIds.ComfortMode, cancellationToken);

        return (int)comfortModePoint.Value switch
        {
            0 => ComfortMode.Economy,
            1 => ComfortMode.Normal,
            2 => ComfortMode.Luxury,
            _ => throw new InvalidOperationException($"Failed to translate comfort mode value: {comfortModePoint.Value}")
        };
    }

    public async Task<OpMode> GetOpMode(CancellationToken cancellationToken)
    {
        var comfortModePoint = await GetPoint(NibeParameterIds.OpMode, cancellationToken);

        return (int)comfortModePoint.Value switch
        {
            0 => OpMode.Auto,
            1 => OpMode.Manual,
            2 => OpMode.AddHeatOnly,
            _ => throw new InvalidOperationException($"Failed to translate OpMode value: {comfortModePoint.Value}")
        };
    }

    public async Task<double> GetExhaustAirTemp(CancellationToken cancellationToken)
    {
        var result = await GetPoint(NibeParameterIds.ExhaustAirTemp, cancellationToken);
        return result.Value;
    }

    public async Task<double> GetCurrentOutdoorTemp(CancellationToken cancellationToken)
    {
        var result = await GetPoint(NibeParameterIds.CurrentOutdoorTemp, cancellationToken);
        return result.Value;
    }

    public async Task UpdateHeat(int value, CancellationToken cancellationToken)
    {
        if (value < -10 || value > 10)
        {
            throw new ArgumentException($"Invalid value: {value}. Must be between -10 and 10", nameof(value));
        }

        var currentHeat = await GetPoint(NibeParameterIds.HeatingOffset, cancellationToken);
        if (currentHeat.Value == value)
        {
            return;
        }

        var auditEvent = new AuditEvent
        {
            Description = $"Nibe Värmepump - Värme",
            NewValue = value,
            OldValue = currentHeat.Value,
        };
        await PatchPoint(value, NibeParameterIds.HeatingOffset, auditEvent, cancellationToken);
    }

    public async Task UpdateComfortMode(ComfortMode value, CancellationToken cancellationToken)
    {
        var currentValue = await GetComfortMode(cancellationToken);
        if (currentValue == value)
        {
            return;
        }

        var auditEvent = new AuditEvent
        {
            Description = $"Nibe Värmepump - Varmvatten",
            NewValue = value.ToString(),
            OldValue = currentValue.ToString(),
        };
        await PatchPoint(value, NibeParameterIds.ComfortMode, auditEvent, cancellationToken);
    }

    public async Task UpdateOpMode(OpMode value, CancellationToken cancellationToken)
    {
        var currentValue = await GetOpMode(cancellationToken);
        if (currentValue == value)
        {
            return;
        }

        var auditEvent = new AuditEvent
        {
            Description = $"Nibe Värmepump - Driftläge elpatron",
            NewValue = value.ToString(),
            OldValue = currentValue.ToString(),
        };
        await PatchPoint(value, NibeParameterIds.OpMode, auditEvent, cancellationToken);
    }

    public async Task UpdateIn­creasedVenti­lation(IncreasedVentilationValue value, CancellationToken cancellationToken)
    {
        var currentValue = await GetPoint(NibeParameterIds.IncreasedVentilation, cancellationToken);
        if (currentValue.Value == (double)value)
        {
            return;
        }

        var auditEvent = new AuditEvent
        {
            Description = $"Nibe Värmepump - Förhöjd Ventilation",
            NewValue = value.ToString(),
            OldValue = currentValue.ToString(),
        };
        await PatchPoint(value, NibeParameterIds.IncreasedVentilation, auditEvent, cancellationToken);
    }

    private async Task<NibePoint> GetPoint(int pointId, CancellationToken cancellationToken)
    {
        var uri = GetDevicePointsApiPath();
        var cacheKey = GetDevicePointsCacheKey(uri);

        var allPoints = await _memoryCache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2);

            return await _httpClient.GetFromJsonAsync<IEnumerable<NibePoint>>(uri, cancellationToken)
                ?? throw new InvalidOperationException($"Failed to get heat points from uri {uri}");
        });

        return allPoints?.SingleOrDefault(p => p.ParameterId == pointId.ToString())
            ?? throw new InvalidOperationException($"Could not find heat point with id {pointId} from uri {uri}");
    }

    private async Task PatchPoint(object value, int point, AuditEvent auditEvent, CancellationToken cancellationToken)
    {
        var requestBody = new Dictionary<string, object>
        {
            { point.ToString(), value }
        };

        var uri = GetDevicePointsApiPath();

        var response = await _httpClient.PatchAsJsonAsync(uri, requestBody, cancellationToken);
        response.EnsureSuccessStatusCode();

        await _auditRepository.UpsertAsync(auditEvent);

        var cacheKey = GetDevicePointsCacheKey(uri);
        _memoryCache.Remove(cacheKey);
    }

    private static string GetDevicePointsCacheKey(Uri uri) => $"NibeDevicePoints_{uri}";

    private Uri GetDevicePointsApiPath() => new($"{_options.Value.BaseAddress}/v2/devices/emmy-r-208006-20240516-06605519022003-54-10-ec-c4-ca-9a/points");
}
