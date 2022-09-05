namespace Stepping.TmProviders.LocalTm.DistributedLocks;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S3881:\"IDisposable\" should be implemented correctly", Justification = "<Pending>")]
public class DefaultSteppingDistributedLockHandle : ISteppingDistributedLockHandle
{
    public void Dispose()
    {
        // Method intentionally left empty.
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}