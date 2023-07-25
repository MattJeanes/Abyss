using Abyss.Web.Helpers.Interfaces;

namespace Abyss.Web.Services;

public class BackgroundWorkerService : BackgroundService
{
    private readonly IBackgroundTaskQueue _queue;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<BackgroundWorkerService> _logger;

    public BackgroundWorkerService(IBackgroundTaskQueue queue, IServiceScopeFactory serviceScopeFactory, ILogger<BackgroundWorkerService> logger)
    {
        _queue = queue;
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var workItem = await _queue.DequeueAsync(cancellationToken);

            await using var scope = _serviceScopeFactory.CreateAsyncScope();
            try
            {
                await workItem(scope.ServiceProvider, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to run background job");
            }
        }
    }
}
