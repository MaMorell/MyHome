using Tibber.Sdk;

namespace MyHome.ApiService.HostedServices;

public sealed class EnergyConsumptionHost(
    ILogger<EnergyConsumptionHost> logger,
    IObserver<RealTimeMeasurement> observer,
    IServiceScopeFactory serviceScopeFactory) : IHostedService, IDisposable
{
    private readonly ILogger<EnergyConsumptionHost> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IObserver<RealTimeMeasurement> _observer = observer ?? throw new ArgumentNullException(nameof(observer));
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));

    private Guid _homeId;
    private IDisposable? _listenerSubscription;
    private IServiceScope? _currentScope;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("{Service} is starting", nameof(EnergyConsumptionHost));

        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var tibberClient = scope.ServiceProvider.GetRequiredService<TibberApiClient>();

            await tibberClient.ValidateRealtimeDevice(cancellationToken);
            _homeId = await GetHomeId(tibberClient, cancellationToken);
            await StartListener(cancellationToken);

            _logger.LogInformation("{Service} started successfully", nameof(EnergyConsumptionHost));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start {Service}", nameof(EnergyConsumptionHost));
            throw;
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("{Service} is stopping", nameof(EnergyConsumptionHost));

        try
        {
            await StopListener(cancellationToken);
            _logger.LogInformation("{Service} stopped successfully", nameof(EnergyConsumptionHost));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping {Service}", nameof(EnergyConsumptionHost));
            throw;
        }
    }

    private async Task StartListener(CancellationToken cancellationToken)
    {
        try
        {
            _currentScope = _serviceScopeFactory.CreateScope();
            var tibberClient = _currentScope.ServiceProvider.GetRequiredService<TibberApiClient>();

            var listener = await tibberClient.StartRealTimeMeasurementListener(_homeId, cancellationToken);
            _listenerSubscription = listener.Subscribe(_observer);
            _logger.LogInformation("Real Time Measurement listener started");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start Real Time Measurement listener");
            _listenerSubscription?.Dispose();
            _currentScope?.Dispose();
            _currentScope = null;
            throw;
        }
    }

    private async Task StopListener(CancellationToken cancellationToken)
    {
        try
        {
            if (_currentScope != null)
            {
                var tibberClient = _currentScope.ServiceProvider.GetRequiredService<TibberApiClient>();
                await tibberClient.StopRealTimeMeasurementListener(_homeId);
            }

            _listenerSubscription?.Dispose();
            _currentScope?.Dispose();
            _currentScope = null;

            _logger.LogInformation("Real Time Measurement listener stopped");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to stop Real Time Measurement listener");
            throw;
        }
    }

    private static async Task<Guid> GetHomeId(TibberApiClient tibberApiClient, CancellationToken cancellationToken)
    {
        var basicData = await tibberApiClient.GetBasicData(cancellationToken);
        var home = basicData.Data.Viewer.Homes.First();
        if (home is null || home.Id is null)
        {
            throw new FormatException("Home or home ID returned from Tibber is null");
        }

        return home.Id.Value;
    }

    public void Dispose()
    {
        _listenerSubscription?.Dispose();
        _currentScope?.Dispose();
    }
}