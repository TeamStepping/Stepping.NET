using Stepping.Core.Databases;

namespace Stepping.Core.Jobs;

public interface IDistributedJobFactory
{
    Task<IDistributedJob> CreateJobAsync(string gid, IDbTransactionContext? dbTransactionContext);

    Task<IAdvancedDistributedJob> CreateAdvancedJobAsync(string gid, IDbTransactionContext? dbTransactionContext);
}