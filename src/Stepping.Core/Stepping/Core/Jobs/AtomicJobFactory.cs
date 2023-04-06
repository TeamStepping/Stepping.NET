using Microsoft.Extensions.DependencyInjection;
using Stepping.Core.Databases;

namespace Stepping.Core.Jobs;

public class AtomicJobFactory : IAtomicJobFactory
{
    protected IServiceProvider ServiceProvider { get; }

    public AtomicJobFactory(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    public virtual async Task<IAtomicJob> CreateJobAsync(string? gid, ISteppingDbContext? dbContext)
    {
        return new AtomicJob(gid ?? await GenerateGidAsync(), dbContext, ServiceProvider);
    }

    public virtual async Task<IAdvancedAtomicJob> CreateAdvancedJobAsync(string? gid,
        ISteppingDbContext? dbContext)
    {
        return new AtomicJob(gid ?? await GenerateGidAsync(), dbContext, ServiceProvider);
    }

    protected virtual async Task<string> GenerateGidAsync()
    {
        var generator = ServiceProvider.GetRequiredService<IAtomicJobGidGenerator>();

        return await generator.CreateAsync();
    }
}