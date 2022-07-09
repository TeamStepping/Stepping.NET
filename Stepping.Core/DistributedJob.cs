namespace Stepping.Core;

public class DistributedJob : IAdvancedDistributedJob
{
    public string Gid { get; }
    public List<IDistributedJobStep> Steps { get; } = new();
    public IDbTransactionContext? DbTransactionContext { get; }

    protected IServiceProvider ServiceProvider { get; }
    protected ITmClient TmClient { get; }
    protected IDbBarrierInserter DbBarrierInserter { get; }
    protected IBarrierInfoModelFactory BarrierInfoModelFactory { get; }

    /// <summary>
    /// You should set <see cref="DbTransactionContext"/> for final consistency
    /// when the current session has DB-write operations in the DB transaction.
    /// </summary>
    public DistributedJob(
        string gid,
        IDbTransactionContext? dbTransactionContext,
        IServiceProvider serviceProvider)
    {
        Gid = gid;
        DbTransactionContext = dbTransactionContext;
        ServiceProvider = serviceProvider;
        TmClient = (ITmClient)serviceProvider.GetService(typeof(ITmClient))!;
        DbBarrierInserter = (IDbBarrierInserter)serviceProvider.GetService(typeof(IDbBarrierInserter))!;
        BarrierInfoModelFactory =
            (IBarrierInfoModelFactory)serviceProvider.GetService(typeof(IBarrierInfoModelFactory))!;
    }

    public virtual Task AddStepAsync<TArgs>(Func<IServiceProvider, TArgs, Task> action, TArgs args)
    {
        Steps.Add(new DistributedJobStep<TArgs>(action, args));

        return Task.CompletedTask;
    }

    public virtual async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        // P1. Send "prepare" to TM.
        await TmPrepareAsync(cancellationToken);

        // P2. Insert a barrier record to DB.
        await DbInsertBarrierAsync(cancellationToken);

        // P3. Commit the DB transaction.
        await DbCommitAsync(cancellationToken);

        // P4. Send "submit" to TM.
        await TmSubmitAsync(cancellationToken);
    }

    public virtual async Task PrepareAndInsertBarrierAsync(CancellationToken cancellationToken = default)
    {
        // P1. Send "prepare" to TM.
        await TmPrepareAsync(cancellationToken);

        // P2. Insert a barrier record to DB.
        await DbInsertBarrierAsync(cancellationToken);
    }

    public virtual async Task SubmitAsync(CancellationToken cancellationToken = default)
    {
        // P4. Send "submit" to TM.
        await TmSubmitAsync(cancellationToken);
    }

    protected virtual async Task TmPrepareAsync(CancellationToken cancellationToken = default)
    {
        await TmClient.PrepareAsync(this, cancellationToken);
    }

    protected virtual async Task DbInsertBarrierAsync(CancellationToken cancellationToken = default)
    {
        if (DbTransactionContext is null)
        {
            throw new SteppingException("DB Transaction not set.");
        }

        await DbBarrierInserter.MustInsertBarrierAsync(
            await BarrierInfoModelFactory.CreateForCommitAsync(this),
            DbTransactionContext.DbContext,
            cancellationToken);
    }

    protected virtual async Task DbCommitAsync(CancellationToken cancellationToken = default)
    {
        if (DbTransactionContext is null)
        {
            throw new SteppingException("DB Transaction not set.");
        }

        await DbTransactionContext.CommitAsync(cancellationToken);
    }

    protected virtual async Task TmSubmitAsync(CancellationToken cancellationToken = default)
    {
        await TmClient.SubmitAsync(this, cancellationToken);
    }
}