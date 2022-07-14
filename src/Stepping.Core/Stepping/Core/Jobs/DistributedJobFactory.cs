using Stepping.Core.Databases;

namespace Stepping.Core.Jobs;

public class DistributedJobFactory : IDistributedJobFactory
{
    protected IServiceProvider ServiceProvider { get; }

    public DistributedJobFactory(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    public virtual Task<IDistributedJob> CreateJobAsync(string gid, ISteppingDbContext? dbContext)
    {
        return Task.FromResult<IDistributedJob>(new DistributedJob(gid, dbContext, ServiceProvider));
    }

    public virtual Task<IAdvancedDistributedJob> CreateAdvancedJobAsync(string gid, ISteppingDbContext? dbContext)
    {
        return Task.FromResult<IAdvancedDistributedJob>(new DistributedJob(gid, dbContext, ServiceProvider));
    }
}