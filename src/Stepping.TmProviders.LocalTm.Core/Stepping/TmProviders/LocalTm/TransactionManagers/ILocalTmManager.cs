using Stepping.Core.Jobs;

namespace Stepping.TmProviders.LocalTm.TransactionManagers;

public interface ILocalTmManager
{
    Task PrepareAsync(IAtomicJob job, CancellationToken cancellationToken = default);

    Task SubmitAsync(IAtomicJob job, CancellationToken cancellationToken = default);

    Task ProcessPendingAsync(CancellationToken cancellationToken = default);

    Task ProcessSubmittedAsync(IAtomicJob job, CancellationToken cancellationToken = default);
}