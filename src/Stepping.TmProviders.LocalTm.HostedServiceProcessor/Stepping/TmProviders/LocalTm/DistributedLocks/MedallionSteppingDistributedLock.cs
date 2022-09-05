using Medallion.Threading;

namespace Stepping.TmProviders.LocalTm.DistributedLocks;

public class MedallionSteppingDistributedLock : ISteppingDistributedLock
{
    protected IDistributedLockProvider DistributedLockProvider { get; }

    public MedallionSteppingDistributedLock(IDistributedLockProvider distributedLockProvider)
    {
        DistributedLockProvider = distributedLockProvider;
    }

    /// <summary>
    /// TryAcquire lock
    /// </summary>
    /// <param name="resource">lock resource</param>
    /// <param name="timeout">How long to wait before giving up on the acquisition attempt. Defaults to 0</param>
    /// <param name="cancellationToken"></param>
    /// <returns>When the acquire lock fail, a null value is returned.</returns>
    public virtual async Task<ISteppingDistributedLockHandle?> TryAcquireAsync(string resource, TimeSpan timeout = default, CancellationToken cancellationToken = default)
    {
        var handle = await DistributedLockProvider.TryAcquireLockAsync(resource, timeout, cancellationToken);

        if (handle == null)
        {
            return null;
        }

        return new MedallionSteppingDistributedLockHandle(handle);
    }
}
