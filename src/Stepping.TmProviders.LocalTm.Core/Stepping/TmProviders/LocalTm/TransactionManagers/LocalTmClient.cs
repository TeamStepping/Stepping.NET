using Stepping.Core.Databases;
using Stepping.Core.Jobs;
using Stepping.Core.TransactionManagers;
using Stepping.TmProviders.LocalTm.Steps;

namespace Stepping.TmProviders.LocalTm.TransactionManagers;

public class LocalTmClient : ITmClient
{
    protected ILocalTmStepConverter LocalTmStepConverter { get; }

    protected ILocalTmManager LocalTmManager { get; }

    protected ISteppingDbContextLookupInfoProvider DbContextLookupInfoProvider { get; }

    public LocalTmClient(
        ILocalTmStepConverter localTmStepConverter,
        ILocalTmManager localTmManager,
        ISteppingDbContextLookupInfoProvider dbContextLookupInfoProvider)
    {
        LocalTmStepConverter = localTmStepConverter;
        LocalTmManager = localTmManager;
        DbContextLookupInfoProvider = dbContextLookupInfoProvider;
    }

    public virtual async Task PrepareAsync(IDistributedJob job, CancellationToken cancellationToken = default)
    {
        await LocalTmManager.PrepareAsync(job, cancellationToken);
    }

    public virtual async Task SubmitAsync(IDistributedJob job, CancellationToken cancellationToken = default)
    {
        await LocalTmManager.SubmitAsync(job, cancellationToken);

        await LocalTmManager.ProcessSubmittedAsync(job, cancellationToken);
    }
}
