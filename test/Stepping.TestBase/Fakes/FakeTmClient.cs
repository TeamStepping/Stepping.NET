using Stepping.Core.Jobs;
using Stepping.Core.TransactionManagers;

namespace Stepping.TestBase.Fakes;

public class FakeTmClient : ITmClient
{
    public Task PrepareAsync(IDistributedJob job, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task SubmitAsync(IDistributedJob job, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}