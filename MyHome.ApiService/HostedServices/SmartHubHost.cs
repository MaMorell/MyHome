using MyHome.ApiService.HostedServices.Services;
using MyHome.Core.Interfaces;

namespace MyHome.ApiService.HostedServices;

public sealed class SmartHubHost : BackgroundService
{
    private readonly ISmartHubClient _smartHubClient;
    private readonly ILogger<SmartHubHost> _logger;

    public SmartHubHost(ISmartHubClient smartHubClient, ILogger<SmartHubHost> logger)
    {
        _smartHubClient = smartHubClient;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation($"{nameof(SmartHubHost)} is starting.");

        try
        {
            await _smartHubClient.StartAsync();
            _logger.LogInformation("SmartHubClient started successfully.");

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (TaskCanceledException)
        {
            _logger.LogInformation("SmartHubHost operation cancelled by application shutdown.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SmartHubHost encountered an error during execution.");
        }

        _logger.LogInformation("SmartHubHost is stopping.");
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("SmartHubHost is performing a graceful stop.");
        try
        {
            await _smartHubClient.StopAsync();
            _logger.LogInformation("SmartHubClient stopped successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping SmartHubClient.");
        }

        await base.StopAsync(cancellationToken);
        _logger.LogInformation("SmartHubHost has stopped.");
    }
}