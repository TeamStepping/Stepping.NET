using Stepping.Core.Databases;

namespace Stepping.Core.Jobs;

public class DistributedJobFactory : IDistributedJobFactory
{
    protected IServiceProvider ServiceProvider { get; }

    public DistributedJobFactory(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    public virtual Task<IDistributedJob> CreateJobAsync(string gid, IDbTransactionContext? dbTransactionContext)
    {
        return Task.FromResult<IDistributedJob>(new DistributedJob(gid, dbTransactionContext, ServiceProvider));
    }

    public virtual Task<IAdvancedDistributedJob> CreateAdvancedJobAsync(string gid,
        IDbTransactionContext? dbTransactionContext)
    {
        return Task.FromResult<IAdvancedDistributedJob>(new DistributedJob(gid, dbTransactionContext, ServiceProvider));
    }
}