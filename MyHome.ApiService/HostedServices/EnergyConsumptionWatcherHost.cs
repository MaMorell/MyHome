using Cronos;
using MyHome.ApiService.Extensions;
using MyHome.ApiService.HostedServices.Services;

namespace MyHome.ApiService.HostedServices;

public sealed class EnergyConsumptionWatcherHost : BackgroundService
{
    private readonly CronExpression _cron = CronExpression.Parse("0/15 * * * *"); // Every 15 minutes
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<EnergyConsumptionWatcherHost> _logger;

    public EnergyConsumptionWatcherHost(
        IServiceScopeFactory scopeFactory,
        ILogger<EnergyConsumptionWatcherHost> logger)
    {
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("{Service} is running.", nameof(HeatRegulatorHost));

        try
        {
            await DoWorkAsync(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                var delay = _cron.GetTimeUntilNextOccurrence();
                await Task.Delay(delay, stoppingToken);

                await DoWorkAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("{Service} is stopping.", nameof(HeatRegulatorHost));
        }
    }

    private async Task DoWorkAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("{Service} is working.", nameof(EnergyConsumptionWatcherHost));

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var host = scope.ServiceProvider.GetRequiredService<EnergyConsumptionListener>();

            await host.StartAsync(cancellationToken);

            await Task.Delay(TimeSpan.FromMinutes(5), cancellationToken);

            await host.StopAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Service} failed.", nameof(EnergyConsumptionWatcherHost));
        }
    }
}
