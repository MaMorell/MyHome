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
    
    private readonly AuditedHttpClient<MyUplinkOptions> _externalHttpClient = externalHttpClient;

    public async Task<HeatPumpPoint> GetHeat(CancellationToken cancellationToken)
    {
        var uri = GetDevicePointsApiPath();

        var result = await _externalHttpClient.GetAsync<IEnumerable<HeatPumpPoint>>(uri, cancellationToken);

        return result.SingleOrDefault(result => result.ParameterId == HeatingOffsetPoint.ToString())
            ?? throw new InvalidOperationException($"Could not find heat point with id {HeatingOffsetPoint}");
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
        const int ComfortModePoint = 47041;

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
            TargetName = "NIBE Värmepump"
        };

        var uri = GetDevicePointsApiPath();
        await _externalHttpClient.PatchAsync(requestBody, uri, auditEvent, cancellationToken);
    }

    private static string GetDevicePointsApiPath()
    {
        const string DEVICE_ID = "emmy-r-208006-20240516-06605519022003-54-10-ec-c4-ca-9a";

        return $"v2/devices/{DEVICE_ID}/points";
    }
}
