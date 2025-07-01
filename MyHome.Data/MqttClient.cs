using Microsoft.Extensions.Logging;
using MQTTnet;
using MyHome.Core.Interfaces;
using MyHome.Core.Models.Entities;
using MyHome.Core.Models.Entities.Constants;
using System.Text;
using System.Text.Json;

namespace MyHome.Data;

public class MqttClient : ISmartHubClient
{
    private readonly IRepository<SensorData> _repository;
    private readonly ILogger<MqttClient> _logger;
    private readonly IMqttClient _mqttClient;
    private readonly MqttClientOptions _mqttClientOptions;
    private readonly JsonSerializerOptions _jsonSerializerOptions = new() { PropertyNameCaseInsensitive = true };

    public MqttClient(IRepository<SensorData> repository, ILogger<MqttClient> logger)
    {
        var mqttFactory = new MqttClientFactory();
        _mqttClient = mqttFactory.CreateMqttClient();
        _mqttClientOptions = new MqttClientOptionsBuilder()
            .WithTcpServer("192.168.10.244", 1883)
            .WithClientId(Guid.NewGuid().ToString())
            .Build();
        _repository = repository;
        _logger = logger;
    }

    public async Task StartAsync()
    {
        await ConnectAsync();

        _mqttClient.DisconnectedAsync += OnDisconnectedAsync;
        _mqttClient.ApplicationMessageReceivedAsync += OnMqttMessageReceivedAsync;
    }

    private async Task OnDisconnectedAsync(MqttClientDisconnectedEventArgs args)
    {
        _logger.LogWarning("Disconnected from MQTT broker. Reason: {reason}. Reconnecting in 5 seconds...", args.Reason);
        await Task.Delay(TimeSpan.FromSeconds(5));
        try
        {
            await ConnectAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Reconnection failed. Will try again...");
        }
    }

    private async Task ConnectAsync()
    {
        var response = await _mqttClient.ConnectAsync(_mqttClientOptions, CancellationToken.None);
        _logger.LogInformation($"Connected to MQTT broker. Result code: {response.ResultCode}");

        var topicFilterBuilder = new MqttTopicFilterBuilder().WithTopic($"zigbee2mqtt/{SensorNameConstants.HumiditySensorBasement}");
        await _mqttClient.SubscribeAsync(topicFilterBuilder.Build());
    }

    public async Task StopAsync()
    {
        await _mqttClient.DisconnectAsync();
        _mqttClient.Dispose();
    }

    private async Task OnMqttMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs args)
    {
        var topic = args.ApplicationMessage.Topic;
        var payload = Encoding.UTF8.GetString(args.ApplicationMessage.Payload);

        _logger.LogDebug("Received message on topic: {topic}", topic);
        _logger.LogTrace("Payload: {payload}", payload);

        if (!topic.StartsWith($"zigbee2mqtt/{SensorNameConstants.HumiditySensorBasement}"))
        {
            _logger.LogWarning("Received message on unexpected topic: {topic}", topic);
            return;
        }

        try
        {
            var sensorData = JsonSerializer.Deserialize<SensorData>(payload, _jsonSerializerOptions);
            if (sensorData == null)
            {
                _logger.LogWarning("Deserialized SensorData was null for topic: {topic}. Payload: {payload}", topic, payload);
                return;
            }

            if (sensorData.Id == Guid.Empty)
            {
                sensorData.Id = Guid.NewGuid();
            }
            sensorData.Timestamp = DateTime.Now;
            sensorData.DeviceName = SensorNameConstants.HumiditySensorBasement;

            await _repository.UpsertAsync(sensorData);
            _logger.LogDebug("Successfully processed and upserted sensor data for topic: {topic}", topic);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Error deserializing JSON from topic {topic}. Payload: {payload}", topic, payload);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred processing message from topic {topic}. Payload: {payload}", topic, payload);
        }
    }
}
