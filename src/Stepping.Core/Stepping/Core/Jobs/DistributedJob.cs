using Microsoft.Extensions.DependencyInjection;
using Stepping.Core.Databases;
using Stepping.Core.Exceptions;
using Stepping.Core.Steps;
using Stepping.Core.TransactionManagers;

namespace Stepping.Core.Jobs;

public class DistributedJob : IAdvancedDistributedJob
{
    public string Gid { get; }
    public List<StepInfoModel> Steps { get; } = new();
    public ITmJobConfigurations? TmOptions { get; set; }
    public IDbTransactionContext? DbTransactionContext { get; }

    protected IServiceProvider ServiceProvider { get; }
    protected ITmClient TmClient { get; }
    protected IStepNameProvider StepNameProvider { get; }
    protected IDbBarrierInserterResolver DbBarrierInserterResolver { get; }
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
        TmClient = serviceProvider.GetRequiredService<ITmClient>();
        StepNameProvider = serviceProvider.GetRequiredService<IStepNameProvider>();
        DbBarrierInserterResolver = serviceProvider.GetRequiredService<IDbBarrierInserterResolver>();
        BarrierInfoModelFactory = serviceProvider.GetRequiredService<IBarrierInfoModelFactory>();
    }

    public virtual async Task AddStepAsync<TStep, TArgs>(TArgs args) where TStep : IStep<TArgs> where TArgs : class
    {
        Steps.Add(new StepInfoModel(await StepNameProvider.GetAsync<TStep>(), args));
    }

    public virtual async Task AddStepAsync<TStep>() where TStep : IStep
    {
        Steps.Add(new StepInfoModel(await StepNameProvider.GetAsync<TStep>(), null));
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

        var dbBarrierInserter = await DbBarrierInserterResolver.ResolveAsync(DbTransactionContext.DbContext);

        await dbBarrierInserter.MustInsertBarrierAsync(
            await BarrierInfoModelFactory.CreateForCommitAsync(Gid),
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