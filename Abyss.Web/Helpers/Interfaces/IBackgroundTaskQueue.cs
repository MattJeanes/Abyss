namespace Abyss.Web.Helpers.Interfaces;

public interface IBackgroundTaskQueue
{
    void Queue(Func<IServiceProvider, CancellationToken, Task> workItem);
    Task<Func<IServiceProvider, CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken);
}