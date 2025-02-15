using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyHome.Core.Http;
using MyHome.Core.Models;
using MyHome.Core.Models.WifiSocket;
using MyHome.Core.Options;

namespace MyHome.Core.Repositories.WifiSocket;

public class WifiSocketClient(
    AuditedHttpClient<WifiSocketOptions> externalHttpClient,
    IOptions<WifiSocketOptions> wifiSocketOptions,
    ILogger<WifiSocketClient> logger)
{
    private const string STATUS_OK = "ok";

    private readonly AuditedHttpClient<WifiSocketOptions> _externalHttpClient = externalHttpClient;
    private readonly WifiSocketOptions _wifiSocketOptions = wifiSocketOptions?.Value ?? throw new ArgumentNullException(nameof(wifiSocketOptions));
    private readonly ILogger<WifiSocketClient> _logger = logger;

    public string Name => _wifiSocketOptions.Name;

    public async Task<ControllStatus> GetStatus(CancellationToken cancellationToken = default)
    {
        var response = await _externalHttpClient.GetAsync<ControllStatus>("control-status", cancellationToken);

        response.Name = _wifiSocketOptions.Name;

        return response;
    }

    public Task<bool> UpdateHeat(int value, CancellationToken cancellationToken)
    {
        if (value < 5 || value > 20)
        {
            throw new ArgumentException($"Invalid value: {value}. Must be between 5 and 20", nameof(value));
        }

        return UpdateHeatAsync(value, cancellationToken);
    }

    private async Task<bool> UpdateHeatAsync(int value, CancellationToken cancellationToken)
    {
        var status = await GetStatus(cancellationToken);

        if (status.Status != STATUS_OK)
        {
            _logger.LogWarning($"Radiator is not in a valid state for temperature update. Current status: {status.Status}. Expected status: {STATUS_OK}");
            return false;
        }

        if (status.SetTemperature == value)
        {
            return true;
        }

        return await SetTemprature(value, cancellationToken);
    }

    private async Task<bool> SetTemprature(int value, CancellationToken cancellationToken)
    {
        var auditEvent = new AuditEvent(AuditAction.Update, AuditTarget.WifiSocket)
        {
            NewValue = value,
            TargetName = Name.ToString()
        };

        var requestBody = new SetTemprature()
        {
            Value = value
        };

        var setTempratureResponse = await _externalHttpClient.PostAsync<SetTempratureResponse>(requestBody, "set-temperature", auditEvent, cancellationToken);

        return EvaluateSetTempratureResponse(setTempratureResponse);
    }

    private bool EvaluateSetTempratureResponse(SetTempratureResponse? setTempratureResponse)
    {
        if (setTempratureResponse != null && setTempratureResponse.Status != "ok")
        {
            _logger.LogWarning("Set temprature failed. Expected response 'ok' but got '{setTempratureResponse}'", setTempratureResponse);
            return false;
        }

        return true;
    }
}
