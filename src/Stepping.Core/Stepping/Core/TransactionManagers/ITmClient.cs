using Stepping.Core.Jobs;

namespace Stepping.Core.TransactionManagers;

public interface ITmClient
{
    Task PrepareAsync(IDistributedJob job, CancellationToken cancellationToken = default);

    Task SubmitAsync(IDistributedJob job, CancellationToken cancellationToken = default);
}