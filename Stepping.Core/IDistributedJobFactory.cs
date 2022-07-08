using System.Data;

namespace Stepping.Core;

public interface IDistributedJobFactory
{
    Task<IDistributedJob> CreateJobAsync(string gid, IDbTransactionContext? dbTransactionContext);

    Task<IAdvancedDistributedJob> CreateAdvancedJobAsync(string gid, IDbTransactionContext? dbTransactionContext);
}