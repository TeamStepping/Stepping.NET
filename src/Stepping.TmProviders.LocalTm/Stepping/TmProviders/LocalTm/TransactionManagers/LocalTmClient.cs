using Stepping.Core.Databases;
using Stepping.Core.Jobs;
using Stepping.Core.TransactionManagers;
using Stepping.TmProviders.LocalTm.Steps;

namespace Stepping.TmProviders.LocalTm.TransactionManagers;

public class LocalTmClient : ITmClient
{
    protected ILocalTmStepConverter LocalTmStepConverter { get; }

    protected ILocalTmProcessor LocalTmProcessor { get; }

    protected ISteppingDbContextLookupInfoProvider DbContextLookupInfoProvider { get; }

    public LocalTmClient(
        ILocalTmStepConverter localTmStepConverter,
        ILocalTmProcessor localTmProcessor,
        ISteppingDbContextLookupInfoProvider dbContextLookupInfoProvider)
    {
        LocalTmStepConverter = localTmStepConverter;
        LocalTmProcessor = localTmProcessor;
        DbContextLookupInfoProvider = dbContextLookupInfoProvider;
    }

    public virtual async Task PrepareAsync(IDistributedJob job, CancellationToken cancellationToken = default)
    {
        var steps = await LocalTmStepConverter.ConvertAsync(job.Steps);
        var steppingDbContextLookupInfo = await DbContextLookupInfoProvider.GetAsync(job.DbContext!);

        await LocalTmProcessor.PrepareAsync(job.Gid, steps, steppingDbContextLookupInfo, cancellationToken);
    }

    public virtual async Task SubmitAsync(IDistributedJob job, CancellationToken cancellationToken = default)
    {
        await LocalTmProcessor.SubmitAsync(job.Gid, cancellationToken);

        await LocalTmProcessor.ProcessSubmitAsync(job.Gid, cancellationToken);
    }
}
