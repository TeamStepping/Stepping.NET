using Stepping.Core.Databases;

namespace Stepping.Core.Jobs;

public static class DistributedJobFactoryExtensions
{
    public static Task<IDistributedJob> CreateJobAsync(this IDistributedJobFactory factory,
        ISteppingDbContext dbContext) => factory.CreateJobAsync(null, dbContext);

    public static Task<IAdvancedDistributedJob> CreateAdvancedJobAsync(this IDistributedJobFactory factory,
        ISteppingDbContext dbContext) => factory.CreateAdvancedJobAsync(null, dbContext);
}