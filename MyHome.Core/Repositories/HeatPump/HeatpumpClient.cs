using MyHome.Core.Http;
using MyHome.Core.Models.HeatPump;
using MyHome.Core.Options;

namespace MyHome.Core.Repositories.HeatPump;

public class HeatPumpPoint
{
    public string ParameterId { get; set; } = string.Empty;
    public string ParameterName { get; set; } = string.Empty;
    public double Value { get; set; }
}

public class HeatpumpClient(AuditedHttpClient<MyUplinkOptions> externalHttpClient)
{
    private const int HeatingOffsetPoint = 47011;
    private const int ComfortModePoint = 47041;

    private readonly AuditedHttpClient<MyUplinkOptions> _httpClient = externalHttpClient;

    public async Task<HeatPumpPoint> GetHeat(CancellationToken cancellationToken)
    {
        var uri = GetDevicePointsApiPath();

        var result = await _httpClient.GetAsync<IEnumerable<HeatPumpPoint>>(uri, cancellationToken);

        return result.SingleOrDefault(result => result.ParameterId == HeatingOffsetPoint.ToString())
            ?? throw new InvalidOperationException($"Could not find heat point with id {HeatingOffsetPoint}");
    }

    public async Task<ComfortMode> GetComfortMode(CancellationToken cancellationToken)
    {
        var uri = GetDevicePointsApiPath();

        var response = await _httpClient.GetAsync<IEnumerable<HeatPumpPoint>>(uri, cancellationToken);

        var comfortModePoint = response.SingleOrDefault(result => result.ParameterId == ComfortModePoint.ToString())
            ?? throw new InvalidOperationException($"Could not find heat point with id {HeatingOffsetPoint}");

        return (int)comfortModePoint.Value switch
        {
            0 => ComfortMode.Economy,
            1 => ComfortMode.Normal,
            2 => ComfortMode.Luxury,
            _ => throw new InvalidOperationException($"Invalid comfort mode value: {comfortModePoint.Value}")
        };
    }

    public async Task UpdateHeat(int value, CancellationToken cancellationToken)
    {
        if (value < -10 || value > 10)
        {
            throw new ArgumentException($"Invalid value: {value}. Must be between -10 and 10", nameof(value));
        }

        var currentHeat = await GetHeat(cancellationToken);
        if (currentHeat.Value == value)
        {
            return;
        }

        await PatchPoint(value, HeatingOffsetPoint, cancellationToken);
    }

    public async Task UpdateComfortMode(ComfortMode value, CancellationToken cancellationToken)
    {
        var currentComfortMode = await GetComfortMode(cancellationToken);
        if (currentComfortMode == value)
        {
            return;
        }

        await PatchPoint(value, ComfortModePoint, cancellationToken);
    }

    private async Task PatchPoint(object value, int point, CancellationToken cancellationToken)
    {
        var requestBody = new Dictionary<string, object>
        {
            { point.ToString(), value }
        };

        var auditEvent = new AuditEvent(AuditAction.Update, AuditTarget.HeatPump)
        {
            NewValue = value,
            TargetName = GetAuditTargetName(point)
        };

        var uri = GetDevicePointsApiPath();
        await _httpClient.PatchAsync(requestBody, uri, auditEvent, cancellationToken);
    }

    private static string GetAuditTargetName(int point) => point switch
    {
        ComfortModePoint => "Nibe värmepump - Varmvatten",
        HeatingOffsetPoint => "Nibe värmepump - Värme",
        _ => "NIBE Värmepump",
    };

    private static string GetDevicePointsApiPath()
    {
        const string DEVICE_ID = "emmy-r-208006-20240516-06605519022003-54-10-ec-c4-ca-9a";

        return $"v2/devices/{DEVICE_ID}/points";
    }
}
