using Microsoft.Extensions.Options;
using MQTTnet;
using MyHome.Core.Interfaces;
using MyHome.Core.Models.Integrations.HeatPump;
using MyHome.Data.Integrations.HeatPump.Dtos;
using MyHome.Data.Options;

namespace MyHome.Data.Integrations.HeatPump;

public class NibeClient(IOptions<HeatPumpClientOptions> options) : IHeatPumpClient
{
    public async Task UpdateHeat(int value, CancellationToken cancellationToken)
    {
        if (value < -10 || value > 10)
            throw new ArgumentException($"Invalid value: {value}. Must be between -10 and 10", nameof(value));

        await PublishPoint(NibeParameterIds.HeatingOffset, value.ToString(), cancellationToken);
    }

    public async Task UpdateComfortMode(ComfortMode value, CancellationToken cancellationToken) =>
        await PublishPoint(NibeParameterIds.ComfortMode, ((int)value).ToString(), cancellationToken);

    public async Task UpdateOpMode(OpMode value, CancellationToken cancellationToken) =>
        await PublishPoint(NibeParameterIds.OpMode, ((int)value).ToString(), cancellationToken);

    private async Task PublishPoint(int pointId, string payload, CancellationToken cancellationToken)
    {
        using var client = await CreateConnectedClientAsync(cancellationToken);

        await client.PublishAsync(new MqttApplicationMessageBuilder()
            .WithTopic($"nibe/modbus/{pointId}/set")
            .WithPayload(payload)
            .Build(), cancellationToken);

        await client.DisconnectAsync(cancellationToken: cancellationToken);
    }

    private async Task<IMqttClient> CreateConnectedClientAsync(CancellationToken cancellationToken)
    {
        var mqttOptions = options.Value;
        var factory = new MqttClientFactory();
        var client = factory.CreateMqttClient();

        var connectOptions = new MqttClientOptionsBuilder()
            .WithTcpServer(mqttOptions.MqttHost, mqttOptions.MqttPort)
            .Build();

        await client.ConnectAsync(connectOptions, cancellationToken);
        return client;
    }
}
