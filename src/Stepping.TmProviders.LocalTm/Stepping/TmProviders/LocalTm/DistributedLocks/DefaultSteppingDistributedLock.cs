using System.Collections.Concurrent;

namespace Stepping.TmProviders.LocalTm.DistributedLocks;

public class DefaultSteppingDistributedLock : ISteppingDistributedLock
{
    private static readonly ConcurrentDictionary<string, Lazy<SemaphoreSlim>> _lockerItems = new();

    /// <summary>
    /// TryAcquire lock
    /// </summary>
    /// <param name="resource">lock resource</param>
    /// <param name="timeout">How long to wait before giving up on the acquisition attempt. Defaults to 0</param>
    /// <param name="cancellationToken"></param>
    /// <returns>When the acquire lock fail, a null value is returned.</returns>
    public virtual async Task<ISteppingDistributedLockHandle?> TryAcquireAsync(string resource, TimeSpan timeout = default, CancellationToken cancellationToken = default)
    {
        var slim = GetSemaphoreSlim(resource);

        if (!await slim.WaitAsync(timeout, cancellationToken))
        {
            return null;
        }

        return new DefaultSteppingDistributedLockHandle(slim);
    }

    protected virtual SemaphoreSlim GetSemaphoreSlim(string resource)
    {
        return _lockerItems.GetOrAdd(resource, _ => new Lazy<SemaphoreSlim>(() => new SemaphoreSlim(1, 1), true)).Value;
    }
}
