using Stepping.Core.Databases;

namespace Stepping.Core.Jobs;

public interface IDistributedJob
{
    string Gid { get; }

    List<IDistributedJobStep> Steps { get; }

    IDbTransactionContext? DbTransactionContext { get; }

    /// <summary>
    /// Add a step for the job to do in order.
    /// </summary>
    Task AddStepAsync<TArgs>(Func<IServiceProvider, TArgs, Task> action, TArgs args);

    /// <summary>
    /// Send "prepare" to TM, insert a barrier record to DB, commit the DB transaction, and send "submit" to TM.
    /// Execute only the "submit" sending if the job is not with a DB transaction.
    /// The TM will do the steps you added in order.
    /// </summary>
    Task ExecuteAsync(CancellationToken cancellationToken = default);
}