using Stepping.Core.Jobs;

namespace Stepping.Core.TransactionManagers;

public interface ITmClient
{
    Task PrepareAsync(IAtomicJob job, CancellationToken cancellationToken = default);

    Task SubmitAsync(IAtomicJob job, CancellationToken cancellationToken = default);
}