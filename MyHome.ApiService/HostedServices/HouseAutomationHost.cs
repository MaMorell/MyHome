﻿using Cronos;
using MyHome.ApiService.Extensions;
using MyHome.Core.Services;

namespace MyHome.ApiService.HostedServices;

public sealed class HouseAutomationHost : BackgroundService
{
    private readonly CronExpression _cron = CronExpression.Parse("0 * * * *"); // Every hour;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<HouseAutomationHost> _logger;

    public HouseAutomationHost(
        IServiceScopeFactory scopeFactory,
        ILogger<HouseAutomationHost> logger)
    {
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("{Service} is running.", nameof(HouseAutomationHost));

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
            _logger.LogInformation("{Service} is stopping.", nameof(HouseAutomationHost));
        }
    }

    private async Task DoWorkAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("{Service} is working.", nameof(HouseAutomationHost));

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var houseAutomationService = scope.ServiceProvider.GetRequiredService<HouseAutomationService>();

            await houseAutomationService.UpdateHouseSettings(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Heat regulation cycle failed");
        }
    }
}
