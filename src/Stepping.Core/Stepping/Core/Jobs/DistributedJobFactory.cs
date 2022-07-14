using Microsoft.Extensions.DependencyInjection;
using Stepping.Core.Databases;

namespace Stepping.Core.Jobs;

public class DistributedJobFactory : IDistributedJobFactory
{
    protected IServiceProvider ServiceProvider { get; }

    public DistributedJobFactory(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    public virtual async Task<IDistributedJob> CreateJobAsync(string? gid, ISteppingDbContext? dbContext)
    {
        return new DistributedJob(gid ?? await GenerateGidAsync(), dbContext, ServiceProvider);
    }

    public virtual async Task<IAdvancedDistributedJob> CreateAdvancedJobAsync(string? gid,
        ISteppingDbContext? dbContext)
    {
        return new DistributedJob(gid ?? await GenerateGidAsync(), dbContext, ServiceProvider);
    }

    protected virtual async Task<string> GenerateGidAsync()
    {
        var generator = ServiceProvider.GetRequiredService<IDistributedJobGidGenerator>();

        return await generator.CreateAsync();
    }
}