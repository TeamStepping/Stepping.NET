namespace Stepping.TmProviders.LocalTm.DistributedLocks;

public class DefaultSteppingDistributedLock : ISteppingDistributedLock
{
    private static readonly Dictionary<string, RefCounted<SemaphoreSlim>> _lockerItems = new();

    /// <summary>
    /// TryAcquire lock
    /// </summary>
    /// <param name="resource">lock resource</param>
    /// <param name="timeout">How long to wait before giving up on the acquisition attempt. Defaults to 0</param>
    /// <param name="cancellationToken"></param>
    /// <returns>When the acquire lock fail, a null value is returned.</returns>
    public virtual async Task<ISteppingDistributedLockHandle?> TryAcquireAsync(string resource, TimeSpan timeout = default, CancellationToken cancellationToken = default)
    {
        var slimRef = GetSemaphoreSlimRef(resource);

        if (!await slimRef.Value.WaitAsync(timeout, cancellationToken))
        {
            return null;
        }

        return new DefaultSteppingDistributedLockHandle(() =>
        {
            TryRemoveSemaphoreSlimRef(slimRef);
            slimRef.Value.Dispose();
        });
    }

    protected virtual RefCounted<SemaphoreSlim> GetSemaphoreSlimRef(string resource)
    {
        RefCounted<SemaphoreSlim> _slimRef;
        lock (_lockerItems)
        {
            if (_lockerItems.TryGetValue(resource, out _slimRef!))
            {
                _slimRef.Increase();
            }
            else
            {
                _slimRef = new RefCounted<SemaphoreSlim>(resource, new SemaphoreSlim(1, 1));
                _lockerItems[resource] = _slimRef;
            }
        }
        return _slimRef;
    }

    protected virtual bool TryRemoveSemaphoreSlimRef(RefCounted<SemaphoreSlim> _slimRef)
    {
        lock (_lockerItems)
        {
            _slimRef.Decrease();
            if (_slimRef.RefCount == 0)
            {
                _lockerItems.Remove(_slimRef.Resource);
                return true;
            }
        }

        return false;
    }
}
