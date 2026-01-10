using MyHome.Core.Interfaces;
using MyHome.Core.Models.Audit;
using MyHome.Data.Integrations.Thermostats.Models;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace MyHome.Data.Integrations.Thermostats;

public class EbecoConnectClient : IThermostatClient
{
    private readonly HttpClient _httpClient;
    private readonly IRepository<AuditEvent> _auditRepository;
    private const int ThermostatId = 29438;

    public EbecoConnectClient(HttpClient httpClient, IRepository<AuditEvent> auditRepository)
    {
        _httpClient = httpClient;
        _auditRepository = auditRepository;
    }

    public async Task<double> GetSetTemperatureAsync()
    {
        var userDevice = await GetUserDeviceAsync();
        return userDevice.TemperatureSet;
    }

    public async Task UpdateSetTemperatureAsync(int temperature)
    {
        var userDevice = await GetUserDeviceAsync();
        if (userDevice.TemperatureSet == temperature)
        {
            return;
        }

        var auditEvent = new AuditEvent
        {
            Description = $"{userDevice.DisplayName} - Temperatur",
            NewValue = temperature,
            OldValue = userDevice.TemperatureSet
        };

        var requestBody = new UpdateDeviceInput()
        {
            Id = ThermostatId,
            TemperatureSet = temperature
        };

        var requestBodyJson = JsonSerializer.Serialize(requestBody);
        var url = $"/api/services/app/Devices/UpdateUserDevice";
        var content = new StringContent(requestBodyJson, Encoding.UTF8, "application/json");
        var response = await _httpClient.PutAsync(url, content);
        response.EnsureSuccessStatusCode();

        await _auditRepository.UpsertAsync(auditEvent);
    }

    private async Task<UserDevice> GetUserDeviceAsync()
    {
        var url = $"/api/services/app/Devices/GetUserDeviceById?id={ThermostatId}";
        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var deviceResponse = await response.Content.ReadFromJsonAsync<UserDeviceResponse>(
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        if (deviceResponse?.Result == null || !deviceResponse.Success)
        {
            throw new InvalidOperationException("Failed to deserialize device response or API call was not successful");
        }

        return deviceResponse.Result;
    }
}