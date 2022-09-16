using Stepping.Core.Jobs;

namespace Stepping.TmProviders.LocalTm.TransactionManagers;

public interface ILocalTmManager
{
    Task PrepareAsync(IDistributedJob job, CancellationToken cancellationToken = default);

    Task SubmitAsync(IDistributedJob job, CancellationToken cancellationToken = default);

    Task ProcessPendingAsync(CancellationToken cancellationToken = default);

    Task ProcessSubmittedAsync(IDistributedJob job, CancellationToken cancellationToken = default);
}