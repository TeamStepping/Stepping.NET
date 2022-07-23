namespace Stepping.TmProviders.LocalTm.DistributedLocks;

public interface ISteppingDistributedLock
{
    /// <summary>
    /// TryAcquire lock
    /// </summary>
    /// <param name="resource">lock resource</param>
    /// <param name="timeout">How long to wait before giving up on the acquisition attempt. Defaults to 0</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ISteppingDistributedLockHandle?> TryAcquireAsync(string resource, TimeSpan timeout = default, CancellationToken cancellationToken = default);
}