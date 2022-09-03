namespace Stepping.TmProviders.LocalTm.DistributedLocks;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S3881:\"IDisposable\" should be implemented correctly", Justification = "<Pending>")]
public class DefaultSteppingDistributedLockHandle : ISteppingDistributedLockHandle
{
    private readonly Action _action;

    public DefaultSteppingDistributedLockHandle(Action action)
    {
        _action = action;
    }

    public void Dispose()
    {
        _action.Invoke();
    }

    public ValueTask DisposeAsync()
    {
        _action.Invoke();
        return ValueTask.CompletedTask;
    }
}