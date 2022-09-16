namespace Stepping.TmProviders.LocalTm.DistributedLocks;

public class DefaultSteppingDistributedLock : ISteppingDistributedLock
{
    private static readonly Task<ISteppingDistributedLockHandle?> _taskHandleCache =
        Task.FromResult<ISteppingDistributedLockHandle?>(new DefaultSteppingDistributedLockHandle());

    /// <summary>
    /// TryAcquire lock
    /// </summary>
    /// <param name="resource">lock resource</param>
    /// <param name="timeout">How long to wait before giving up on the acquisition attempt. Defaults to 0</param>
    /// <param name="cancellationToken"></param>
    /// <returns>When the acquire lock fail, a null value is returned.</returns>
    public virtual Task<ISteppingDistributedLockHandle?> TryAcquireAsync(string resource, TimeSpan timeout = default, CancellationToken cancellationToken = default)
    {
        return _taskHandleCache;
    }
}
