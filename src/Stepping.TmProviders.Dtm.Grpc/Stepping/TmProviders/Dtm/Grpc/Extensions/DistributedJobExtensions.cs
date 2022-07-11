using Stepping.Core.Jobs;
using Stepping.TmProviders.Dtm.Grpc.TransactionManagers;

namespace Stepping.TmProviders.Dtm.Grpc.Extensions;

public static class DistributedJobExtensions
{
    public static DtmJobConfigurations GetDtmJobConfigurations(this IDistributedJob job)
    {
        return job.GetOrCreateDtmConfigurations();
    }

    public static IDistributedJob SetRetryInterval(this IDistributedJob job, long retryInterval)
    {
        var options = job.GetOrCreateDtmConfigurations();

        options.RetryInterval = retryInterval;

        return job;
    }

    public static IDistributedJob SetTimeoutToFail(this IDistributedJob job, long timeoutToFail)
    {
        var options = job.GetOrCreateDtmConfigurations();

        options.TimeoutToFail = timeoutToFail;

        return job;
    }

    public static IDistributedJob SetBranchHeader(this IDistributedJob job, string name, string? value)
    {
        var options = job.GetOrCreateDtmConfigurations();

        if (value is null)
        {
            options.BranchHeaders.Remove(name);
        }
        else
        {
            options.BranchHeaders[name] = value;
        }

        return job;
    }

    public static IDistributedJob SetPassthroughHeader(this IDistributedJob job, string name)
    {
        var options = job.GetOrCreateDtmConfigurations();

        options.PassthroughHeaders.Add(name);

        return job;
    }

    private static DtmJobConfigurations GetOrCreateDtmConfigurations(this IDistributedJob job)
    {
        job.TmOptions ??= new DtmJobConfigurations();

        return (DtmJobConfigurations)job.TmOptions;
    }
}