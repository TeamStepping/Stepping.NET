using Stepping.Core.Jobs;
using Stepping.Core.TransactionManagers;

namespace Stepping.TestBase.Fakes;

public class FakeTmClient : ITmClient
{
    public Task PrepareAsync(IAtomicJob job, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task SubmitAsync(IAtomicJob job, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}