using Stepping.Core.Databases;

namespace Stepping.Core.Jobs;

public interface IDistributedJobFactory
{
    Task<IDistributedJob> CreateJobAsync(string? gid = null, ISteppingDbContext? dbContext = null);

    Task<IAdvancedDistributedJob> CreateAdvancedJobAsync(string? gid = null, ISteppingDbContext? dbContext = null);
}