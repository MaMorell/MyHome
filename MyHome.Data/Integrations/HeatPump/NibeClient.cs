using MyHome.Core.Interfaces;
using MyHome.Core.Models.Audit;
using MyHome.Core.Models.Integrations.HeatPump;
using MyHome.Core.Options;
using MyHome.Data.Http;
using MyHome.Data.Integrations.HeatPump.Dtos;

namespace MyHome.Data.Integrations.HeatPump;

public class NibeClient(AuditedHttpClient<MyUplinkOptions> externalHttpClient) : IHeatPumpClient
{
    private readonly AuditedHttpClient<MyUplinkOptions> _httpClient = externalHttpClient;

    public async Task<ComfortMode> GetComfortMode(CancellationToken cancellationToken)
    {
        var comfortModePoint = await GetPoint(NibeParameterIds.ComfortModePoint, cancellationToken);

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

    public async Task UpdateHeat(int value, CancellationToken cancellationToken)
    {
        if (value < -10 || value > 10)
        {
            throw new ArgumentException($"Invalid value: {value}. Must be between -10 and 10", nameof(value));
        }

        var currentHeat = await GetHeatOffset(cancellationToken);
        if (currentHeat.Value == value)
        {
            return;
        }

        await PatchPoint(value, NibeParameterIds.HeatingOffsetPoint, cancellationToken);
    }

    public async Task UpdateComfortMode(ComfortMode value, CancellationToken cancellationToken)
    {
        var currentComfortMode = await GetComfortMode(cancellationToken);
        if (currentComfortMode == value)
        {
            return;
        }

        await PatchPoint(value, NibeParameterIds.ComfortModePoint, cancellationToken);
    }

    public async Task UpdateOpMode(OpMode value, CancellationToken cancellationToken)
    {
        var currentComfortMode = await GetOpMode(cancellationToken);
        if (currentComfortMode == value)
        {
            return;
        }

        await PatchPoint(value, NibeParameterIds.OpMode, cancellationToken);
    }

    private Task<NibePoint> GetHeatOffset(CancellationToken cancellationToken)
    {
        return GetPoint(NibeParameterIds.HeatingOffsetPoint, cancellationToken);
    }

    private async Task<NibePoint> GetPoint(int pointId, CancellationToken cancellationToken)
    {
        var uri = GetDevicePointsApiPath();

        var result = await _httpClient.GetAsync<IEnumerable<NibePoint>>(uri, cancellationToken);

        return result.SingleOrDefault(result => result.ParameterId == pointId.ToString())
            ?? throw new InvalidOperationException($"Could not find heat point with id {pointId} from uri {uri}");
    }

    private async Task PatchPoint(object value, int point, CancellationToken cancellationToken)
    {
        var requestBody = new Dictionary<string, object>
        {
            { point.ToString(), value }
        };

        var auditEvent = new AuditEvent(AuditAction.Update, AuditTarget.HeatPump)
        {
            NewValue = value.ToString(),
            TargetName = GetAuditTargetName(point)
        };

        var uri = GetDevicePointsApiPath();
        await _httpClient.PatchAsync(requestBody, uri, auditEvent, cancellationToken);
    }

    private static string GetAuditTargetName(int point) => point switch
    {
        NibeParameterIds.ComfortModePoint => "Nibe värmepump - Varmvatten",
        NibeParameterIds.HeatingOffsetPoint => "Nibe värmepump - Värme",
        _ => "NIBE Värmepump",
    };

    private static string GetDevicePointsApiPath()
    {
        const string DEVICE_ID = "emmy-r-208006-20240516-06605519022003-54-10-ec-c4-ca-9a";

        return $"v2/devices/{DEVICE_ID}/points";
    }
}
