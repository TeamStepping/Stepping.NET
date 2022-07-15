using Stepping.Core.Databases;

namespace Stepping.Core.Jobs;

public interface IDistributedJobFactory
{
    Task<IDistributedJob> CreateJobAsync(string gid, ISteppingDbContext? dbContext);

    Task<IAdvancedDistributedJob> CreateAdvancedJobAsync(string gid, ISteppingDbContext? dbContext);
}