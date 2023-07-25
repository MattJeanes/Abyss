namespace Abyss.Web.Helpers.Interfaces;

public interface IBackgroundTaskQueue
{
    void Queue(Func<IServiceScopeFactory, CancellationToken, Task> workItem);
    Task<Func<IServiceScopeFactory, CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken);
}