using Contracts.IServices;

namespace WebAPI.BackgroundServices;

public sealed class PointExpiryBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PointExpiryBackgroundService> _logger;

    public PointExpiryBackgroundService(IServiceScopeFactory scopeFactory, ILogger<PointExpiryBackgroundService> logger)
    { _scopeFactory = scopeFactory; _logger = logger; }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var points = scope.ServiceProvider.GetRequiredService<IPointsService>();
                var expiredLots = await points.ExpireDuePointsAsync(stoppingToken);
                _logger.LogInformation("Points expiry job completed; expired {LotCount} point lots", expiredLots);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            catch (Exception ex)
            { _logger.LogError(ex, "Points expiry job failed"); }

            try { await Task.Delay(TimeSpan.FromHours(24), stoppingToken); }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
        }
    }
}
