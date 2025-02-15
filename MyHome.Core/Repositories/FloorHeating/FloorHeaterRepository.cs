using com.clusterrr.TuyaNet;
using Microsoft.Extensions.Options;
using MyHome.Core.Options;
using System.Text.Json;

namespace MyHome.Core.Repositories.FloorHeating;

public class FloorHeaterRepository
{
    private const string CODE_TEMP_SET = "temp_set";

    private readonly TuyaApi _tuyaApi;
    private readonly string _deviceId;

    public FloorHeaterRepository(IOptions<FloorHeaterOptions> options)
    {
        _tuyaApi = new TuyaApi(
            region: TuyaApi.Region.CentralEurope,
            accessId: options.Value.AccessId,
            apiSecret: options.Value.ApiSecret
        );

        _deviceId = options.Value.DeviceId;
    }

    public async Task<double> GetSetTemperatureAsync()
    {
        var statusJson = await _tuyaApi.RequestAsync(TuyaApi.Method.GET, $"/v1.0/devices/{_deviceId}/status");

        var status = JsonSerializer.Deserialize<List<ThermostatStatus>>(statusJson, new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true
        });

        var tempSet = status.FirstOrDefault(x => x.Code == "temp_set")?.Value.GetInt32()
            ?? throw new ThermostatDeviceException("Temperature setting not found in device status");

        return tempSet / 10.0;
    }

    public async Task UpdateSetTemperatureAsync(int temperature)
    {
        var scaledTemprature = ConvertToScaledTemp(temperature);

        var currentSetTemperature = await GetSetTemperatureAsync();
        if (currentSetTemperature == scaledTemprature)
        {
            return;
        }

        await ExecuteTemperatureUpdateAsync(scaledTemprature);
    }

    private async Task ExecuteTemperatureUpdateAsync(int scaledTemprature)
    {
        var command = new
        {
            commands = new[]
            {
                new
                {
                    code = CODE_TEMP_SET,
                    value = scaledTemprature
                }
            }
        };

        var commandJson = JsonSerializer.Serialize(command);
        await _tuyaApi.RequestAsync(TuyaApi.Method.POST, $"/v1.0/devices/{_deviceId}/commands", commandJson);
    }

    private static int ConvertToScaledTemp(int temperature)
    {
        var scaledTemp = temperature * 10;

        if (scaledTemp < 50 || scaledTemp > 990)
        {
            throw new ArgumentException("Temperature must be between 5.0°C and 99.0°C");
        }

        return scaledTemp;
    }

    private class ThermostatStatus
    {
        public string Code { get; set; } = string.Empty;
        public JsonElement Value { get; set; }
    }
}

public class ThermostatDeviceException : Exception
{
    public ThermostatDeviceException(string message) : base(message) { }
    public ThermostatDeviceException(string message, Exception inner) : base(message, inner) { }
}
