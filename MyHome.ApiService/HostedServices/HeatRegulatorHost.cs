using Cronos;
using MyHome.ApiService.Extensions;
using MyHome.Core.Services;

namespace MyHome.ApiService.HostedServices;

public sealed class HeatRegulatorHost : BackgroundService
{
    private readonly CronExpression _cron = CronExpression.Parse("0 * * * *"); // Every hour;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<HeatRegulatorHost> _logger;

    public HeatRegulatorHost(
        IServiceScopeFactory scopeFactory,
        ILogger<HeatRegulatorHost> logger)
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
        _logger.LogInformation("{Service} is working.", nameof(HeatRegulatorHost));

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var heatResulatorService = scope.ServiceProvider.GetRequiredService<HeatRegulatorService>();

            await heatResulatorService.RegulateHeat(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Heat regulation cycle failed");
        }
    }
}
