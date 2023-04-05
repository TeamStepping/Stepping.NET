namespace Stepping.Core.Jobs;

public interface IAdvancedAtomicJob : IAtomicJob
{
    /// <summary>
    /// Send "prepare" to TM, insert a barrier record to DB.
    /// Don't invoke this method if the job is not with a DB transaction.
    /// </summary>
    Task PrepareAndInsertBarrierAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Send "submit" to TM, and then the TM will execute the steps you added in order.
    /// If the job involves a DB transaction, you should commit it yourself before invoking this method.
    /// </summary>
    Task SubmitAsync(CancellationToken cancellationToken = default);
}