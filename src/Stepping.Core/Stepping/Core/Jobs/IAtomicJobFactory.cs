using Stepping.Core.Databases;

namespace Stepping.Core.Jobs;

public interface IAtomicJobFactory
{
    Task<IAtomicJob> CreateJobAsync(string? gid = null, ISteppingDbContext? dbContext = null);

    Task<IAdvancedAtomicJob> CreateAdvancedJobAsync(string? gid = null, ISteppingDbContext? dbContext = null);
}