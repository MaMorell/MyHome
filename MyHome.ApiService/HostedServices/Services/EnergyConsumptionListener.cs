using Tibber.Sdk;

namespace MyHome.ApiService.HostedServices.Services;

public sealed class EnergyConsumptionListener : IDisposable
{
    private readonly TibberApiClient _tibberClient;
    private readonly IObserver<RealTimeMeasurement> _energyConsumptionObserver;
    private readonly ILogger<EnergyConsumptionListener> _logger;

    private Guid _homeId;
    private IDisposable? _listenerSubscription;
    private IServiceScope? _currentScope;

    public EnergyConsumptionListener(TibberApiClient tibberApiClient, IObserver<RealTimeMeasurement> energyConumptioOobserver, ILogger<EnergyConsumptionListener> logger)
    {
        _tibberClient = tibberApiClient;
        _energyConsumptionObserver = energyConumptioOobserver;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("{Service} is starting", nameof(EnergyConsumptionListener));

        await _tibberClient.ValidateRealtimeDevice(cancellationToken);
        _homeId = await GetHomeId(_tibberClient, cancellationToken);
        await StartListener(cancellationToken);

        _logger.LogInformation("{Service} started successfully", nameof(EnergyConsumptionListener));
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("{Service} is stopping", nameof(EnergyConsumptionListener));

        await StopListener(cancellationToken);

        _logger.LogInformation("{Service} stopped successfully", nameof(EnergyConsumptionListener));
    }

    private async Task StartListener(CancellationToken cancellationToken)
    {
        try
        {
            var listener = await _tibberClient.StartRealTimeMeasurementListener(_homeId, cancellationToken);
            _listenerSubscription = listener.Subscribe(_energyConsumptionObserver);
            _logger.LogInformation("Real Time Measurement listener started");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start Real Time Measurement listener");
            Dispose();
            throw;
        }
    }

    private async Task StopListener(CancellationToken cancellationToken)
    {
        if (_currentScope != null)
        {
            var tibberClient = _currentScope.ServiceProvider.GetRequiredService<TibberApiClient>();
            await tibberClient.StopRealTimeMeasurementListener(_homeId);
        }

        Dispose();

        _logger.LogInformation("Real Time Measurement listener stopped");
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
        _currentScope = null;
    }
}