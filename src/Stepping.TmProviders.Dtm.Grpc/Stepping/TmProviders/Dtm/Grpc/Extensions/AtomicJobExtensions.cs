using Stepping.Core.Jobs;
using Stepping.TmProviders.Dtm.Grpc.TransactionManagers;

namespace Stepping.TmProviders.Dtm.Grpc.Extensions;

public static class AtomicJobExtensions
{
    public static DtmJobConfigurations GetDtmJobConfigurations(this IAtomicJob job)
    {
        return job.GetOrCreateDtmConfigurations();
    }

    public static IAtomicJob SetWaitResult(this IAtomicJob job, bool waitResult)
    {
        var options = job.GetOrCreateDtmConfigurations();

        options.WaitResult = waitResult;

        return job;
    }

    public static IAtomicJob SetRetryInterval(this IAtomicJob job, long retryInterval)
    {
        var options = job.GetOrCreateDtmConfigurations();

        options.RetryInterval = retryInterval;

        return job;
    }

    public static IAtomicJob SetTimeoutToFail(this IAtomicJob job, long timeoutToFail)
    {
        var options = job.GetOrCreateDtmConfigurations();

        options.TimeoutToFail = timeoutToFail;

        return job;
    }

    public static IAtomicJob SetBranchHeader(this IAtomicJob job, string name, string? value)
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

    public static IAtomicJob SetPassthroughHeader(this IAtomicJob job, string name)
    {
        var options = job.GetOrCreateDtmConfigurations();

        options.PassthroughHeaders.Add(name);

        return job;
    }

    private static DtmJobConfigurations GetOrCreateDtmConfigurations(this IAtomicJob job)
    {
        job.TmOptions ??= new DtmJobConfigurations();

        return (DtmJobConfigurations)job.TmOptions;
    }
}