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

    private readonly TimeSpan _workingHoursStart = TimeSpan.FromHours(7);  // 07:00
    private readonly TimeSpan _workingHoursEnd = TimeSpan.FromHours(21);   // 19:00
    private readonly TimeSpan _schedulerInterval = TimeSpan.FromMinutes(1);

    private Guid _homeId;
    private Timer? _schedulerTimer;
    private bool _isRunning;
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

            _schedulerTimer = new Timer(
                CheckSchedule,
                null,
                TimeSpan.Zero,
                _schedulerInterval);

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
            if (_schedulerTimer != null)
            {
                await _schedulerTimer.DisposeAsync();
                _schedulerTimer = null;
            }

            if (_isRunning)
            {
                await StopListener(cancellationToken);
            }

            _logger.LogInformation("{Service} stopped successfully", nameof(EnergyConsumptionHost));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping {Service}", nameof(EnergyConsumptionHost));
            throw;
        }
    }

    private async void CheckSchedule(object? state)
    {
        try
        {
            var shouldBeRunning = IsWithinWorkingHours();

            if (shouldBeRunning && !_isRunning)
            {
                await StartListener(CancellationToken.None);
            }
            else if (!shouldBeRunning && _isRunning)
            {
                await StopListener(CancellationToken.None);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in schedule check");
        }
    }

    private async Task StartListener(CancellationToken cancellationToken)
    {
        try
        {
            // Create a new scope for the listener session
            _currentScope?.Dispose();
            _currentScope = _serviceScopeFactory.CreateScope();
            var tibberClient = _currentScope.ServiceProvider.GetRequiredService<TibberApiClient>();

            var listener = await tibberClient.StartRealTimeMeasurementListener(_homeId, cancellationToken);
            _listenerSubscription = listener.Subscribe(_observer);
            _isRunning = true;
            _logger.LogInformation("Real Time Measurement listener started at {Time}", DateTime.Now.ToString("HH:mm"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start Real Time Measurement listener");
            _isRunning = false;
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
            _isRunning = false;

            _logger.LogInformation("Real Time Measurement listener stopped at {Time}", DateTime.Now.ToString("HH:mm"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to stop Real Time Measurement listener");
            throw;
        }
    }

    private bool IsWithinWorkingHours()
    {
        var now = DateTime.Now;
        var currentTime = now.TimeOfDay;

        // Check if it's a working day (Monday-Friday)
        if (now.DayOfWeek == DayOfWeek.Saturday || now.DayOfWeek == DayOfWeek.Sunday)
        {
            return false;
        }

        // Check if current time is within working hours
        return currentTime >= _workingHoursStart && currentTime < _workingHoursEnd;
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
        _schedulerTimer?.Dispose();
        _listenerSubscription?.Dispose();
        _currentScope?.Dispose();
    }
}