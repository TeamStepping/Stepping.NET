using Microsoft.Extensions.DependencyInjection;
using Stepping.Core.Databases;
using Stepping.Core.Exceptions;
using Stepping.Core.Steps;
using Stepping.Core.TransactionManagers;

namespace Stepping.Core.Jobs;

public class DistributedJob : IAdvancedDistributedJob
{
    public virtual string Gid { get; }
    public virtual List<IStep> Steps { get; } = new();
    public virtual ITmJobConfigurations? TmOptions { get; set; }
    public virtual ISteppingDbContext? DbContext { get; }
    public virtual bool PrepareSent { get; protected set; }
    public virtual bool SubmitSent { get; protected set; }

    protected IServiceProvider ServiceProvider { get; }
    protected ITmClient TmClient { get; }
    protected IStepResolver StepResolver { get; }
    protected IDbBarrierInserterResolver DbBarrierInserterResolver { get; }
    protected IBarrierInfoModelFactory BarrierInfoModelFactory { get; }

    /// <summary>
    /// You should set <see cref="DbContext"/> for eventual consistency
    /// when the current session has DB-write operations in the DB transaction.
    /// </summary>
    internal DistributedJob(
        string gid,
        ISteppingDbContext? dbContext,
        IServiceProvider serviceProvider)
    {
        Gid = gid;
        DbContext = dbContext;
        ServiceProvider = serviceProvider;
        TmClient = serviceProvider.GetRequiredService<ITmClient>();
        StepResolver = serviceProvider.GetRequiredService<IStepResolver>();
        DbBarrierInserterResolver = serviceProvider.GetRequiredService<IDbBarrierInserterResolver>();
        BarrierInfoModelFactory = serviceProvider.GetRequiredService<IBarrierInfoModelFactory>();

        if (dbContext is not null)
        {
            CheckTransactionalExists(dbContext);
        }
    }

    public virtual IDistributedJob AddStep<TStep>(TStep step) where TStep : IStep
    {
        Steps.Add(step);

        return this;
    }

    public virtual IDistributedJob AddStep<TStep>() where TStep : IStepWithoutArgs
    {
        Steps.Add(StepResolver.Resolve<TStep>());

        return this;
    }

    public virtual async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (DbContext is not null)
        {
            // P1. Send "prepare" to TM.
            await TmPrepareAsync(cancellationToken);

            // P2. Insert a barrier record to DB.
            await DbInsertBarrierAsync(cancellationToken);

            // P3. Commit the DB transaction.
            await DbCommitAsync(cancellationToken);
        }

        // P4. Send "submit" to TM.
        await TmSubmitAsync(cancellationToken);
    }

    public virtual async Task PrepareAndInsertBarrierAsync(CancellationToken cancellationToken = default)
    {
        CheckDbContextIsNotNull();

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
        CheckStepsExist();

        if (PrepareSent)
        {
            throw new SteppingException("Duplicate sending prepare to TM.");
        }

        await TmClient.PrepareAsync(this, cancellationToken);

        PrepareSent = true;
    }

    protected virtual async Task DbInsertBarrierAsync(CancellationToken cancellationToken = default)
    {
        CheckDbContextIsNotNull();

        var dbBarrierInserter = await DbBarrierInserterResolver.ResolveAsync(DbContext!);

        await dbBarrierInserter.MustInsertBarrierAsync(
            await BarrierInfoModelFactory.CreateForCommitAsync(Gid),
            DbContext!,
            cancellationToken);
    }

    protected virtual async Task DbCommitAsync(CancellationToken cancellationToken = default)
    {
        CheckDbContextIsNotNull();

        await DbContext!.CommitTransactionAsync(cancellationToken);
    }

    protected virtual async Task TmSubmitAsync(CancellationToken cancellationToken = default)
    {
        CheckStepsExist();

        if (SubmitSent)
        {
            throw new SteppingException("Duplicate sending submit to TM.");
        }

        await TmClient.SubmitAsync(this, cancellationToken);

        SubmitSent = true;
    }

    protected virtual void CheckStepsExist()
    {
        if (Steps.Count == 0)
        {
            throw new SteppingException("Steps not set.");
        }
    }

    protected virtual void CheckDbContextIsNotNull()
    {
        if (DbContext is null)
        {
            throw new SteppingException("DB context not set.");
        }
    }

    protected static void CheckTransactionalExists(ISteppingDbContext dbContext)
    {
        if (!dbContext.IsTransactional)
        {
            throw new SteppingException("Specified DB context should be with a transaction.");
        }
    }
}