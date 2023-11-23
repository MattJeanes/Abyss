using Abyss.Web.Helpers.Interfaces;

namespace Abyss.Web.Services;

public class BackgroundWorkerService(IBackgroundTaskQueue queue, IServiceScopeFactory serviceScopeFactory, ILogger<BackgroundWorkerService> logger) : BackgroundService
{
    private readonly IBackgroundTaskQueue _queue = queue;
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
    private readonly ILogger<BackgroundWorkerService> _logger = logger;

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
