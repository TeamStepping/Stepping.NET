namespace Stepping.TmProviders.LocalTm.DistributedLocks;

public class DefaultSteppingDistributedLockHandle : ISteppingDistributedLockHandle
{
    protected SemaphoreSlim SemaphoreSlim;

    public DefaultSteppingDistributedLockHandle(SemaphoreSlim semaphoreSlim)
    {
        SemaphoreSlim = semaphoreSlim;
    }

    public void Dispose()
    {
        SemaphoreSlim.Release();
    }

    public ValueTask DisposeAsync()
    {
        SemaphoreSlim.Release();
        return ValueTask.CompletedTask;
    }
}