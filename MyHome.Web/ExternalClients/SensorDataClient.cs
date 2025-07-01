using MyHome.Core.Models.Entities;

namespace MyHome.Web.ExternalClients;

public class SensorDataClient(ApiServiceClient client)
{
    private const string BaseUri = $"/sensordata";

    public async Task<IEnumerable<SensorData>> GetSensorData(string deviceName, CancellationToken cancellationToken = default)
    {
        var uri = $"{BaseUri}/{deviceName}";

        var result = await client.GetFromJsonAsync<IEnumerable<SensorData>>(uri, cancellationToken);

        return result ?? throw new HttpRequestException($"Failed to get {nameof(SensorData)}");
    }
}
