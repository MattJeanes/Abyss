using Abyss.Web.Helpers.Interfaces;
using System.Collections.Concurrent;

namespace Abyss.Web.Helpers;

public class BackgroundTaskQueue : IBackgroundTaskQueue
{
    private ConcurrentQueue<Func<IServiceScopeFactory, CancellationToken, Task>> _items = new();
    private readonly SemaphoreSlim _signal = new(0);

    public void Queue(Func<IServiceScopeFactory, CancellationToken, Task> workItem)
    {
        if (workItem == null)
        {
            throw new ArgumentNullException(nameof(workItem));
        }

        _items.Enqueue(workItem);
        _signal.Release();
    }

    public async Task<Func<IServiceScopeFactory, CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken)
    {
        await _signal.WaitAsync(cancellationToken);
        _items.TryDequeue(out var workItem);

        return workItem;
    }
}
