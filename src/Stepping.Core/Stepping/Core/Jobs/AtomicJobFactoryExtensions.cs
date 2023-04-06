using Stepping.Core.Databases;

namespace Stepping.Core.Jobs;

public static class AtomicJobFactoryExtensions
{
    public static Task<IAtomicJob> CreateJobAsync(this IAtomicJobFactory factory,
        ISteppingDbContext dbContext) => factory.CreateJobAsync(null, dbContext);

    public static Task<IAdvancedAtomicJob> CreateAdvancedJobAsync(this IAtomicJobFactory factory,
        ISteppingDbContext dbContext) => factory.CreateAdvancedJobAsync(null, dbContext);
}