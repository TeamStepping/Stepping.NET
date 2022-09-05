using Medallion.Threading;

namespace Stepping.TmProviders.LocalTm.DistributedLocks;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S3881:\"IDisposable\" should be implemented correctly", Justification = "<Pending>")]
public class MedallionSteppingDistributedLockHandle : ISteppingDistributedLockHandle
{
    private readonly IDistributedSynchronizationHandle _handle;

    public MedallionSteppingDistributedLockHandle(IDistributedSynchronizationHandle handle)
    {
        _handle = handle;
    }

    public void Dispose()
    {

        _handle.Dispose();
    }

    public ValueTask DisposeAsync()
    {
        return _handle.DisposeAsync();
    }
}