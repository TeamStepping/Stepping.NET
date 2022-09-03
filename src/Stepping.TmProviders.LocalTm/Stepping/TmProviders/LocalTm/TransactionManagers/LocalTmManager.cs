using Microsoft.Extensions.Logging;
using Stepping.Core.Databases;
using Stepping.TmProviders.LocalTm.DistributedLocks;
using Stepping.TmProviders.LocalTm.Models;
using Stepping.TmProviders.LocalTm.Steps;
using Stepping.TmProviders.LocalTm.Store;
using Stepping.TmProviders.LocalTm.Timing;

namespace Stepping.TmProviders.LocalTm.TransactionManagers;

public class LocalTmManager : ILocalTmManager
{
    protected ISteppingDistributedLock DistributedLock { get; }

    protected ITransactionStore TransactionStore { get; }

    protected ILocalTmStepExecutor LocalTmStepExecutor { get; }

    protected ISteppingDbContextProviderResolver DbContextProviderResolver { get; }

    protected IBarrierInfoModelFactory BarrierInfoModelFactory { get; }

    protected IDbBarrierInserterResolver DbBarrierInserterResolver { get; }

    protected ILogger<LocalTmManager> Logger { get; }

    protected ISteppingClock SteppingClock { get; }

    public LocalTmManager(
        ISteppingDistributedLock distributedLock,
        ITransactionStore transactionStore,
        ILocalTmStepExecutor localTmStepExecutor,
        ISteppingDbContextProviderResolver dbContextProviderResolver,
        IBarrierInfoModelFactory barrierInfoModelFactory,
        IDbBarrierInserterResolver dbBarrierInserterResolver,
        ILogger<LocalTmManager> logger,
        ISteppingClock steppingClock)
    {
        DistributedLock = distributedLock;
        TransactionStore = transactionStore;
        LocalTmStepExecutor = localTmStepExecutor;
        DbContextProviderResolver = dbContextProviderResolver;
        BarrierInfoModelFactory = barrierInfoModelFactory;
        DbBarrierInserterResolver = dbBarrierInserterResolver;
        Logger = logger;
        SteppingClock = steppingClock;
    }

    public virtual async Task PrepareAsync(string gid, LocalTmStepModel steps, SteppingDbContextLookupInfoModel steppingDbContextLookupInfo,
        CancellationToken cancellationToken = default)
    {
        var tmTransactionModel = new TmTransactionModel(gid, steps, steppingDbContextLookupInfo);

        await CreateAsync(tmTransactionModel, cancellationToken);

        Logger.LogInformation("Local transaction '{gid}' prepared.", gid);
    }

    public virtual async Task SubmitAsync(string gid, CancellationToken cancellationToken = default)
    {
        var tmTransactionModel = await GetAsync(gid, cancellationToken);

        if (!await IsInStatusAsync(tmTransactionModel, LocalTmConst.StatusPrepare))
        {
            return;
        }

        await UpdateSubmitAsync(tmTransactionModel, cancellationToken);

        Logger.LogInformation("Local transaction '{gid}' committed.", gid);
    }

    public virtual async Task ProcessPendingAsync(CancellationToken cancellationToken = default)
    {
        await using var handle = await DistributedLock.TryAcquireAsync("LocalTm-ProcessPeding", cancellationToken: cancellationToken);

        if (handle == null)
        {
            Logger.LogWarning("Local transaction process pending try acquire lock failed.");
            return;
        }

        var pendingTmTransactionModels = await TransactionStore.GetPendingListAsync(cancellationToken);

        foreach (var tmTransactionModel in pendingTmTransactionModels)
        {
            if (tmTransactionModel.Status == LocalTmConst.StatusPrepare)
            {
                if (await ProcessQueryPreparedAsync(tmTransactionModel.Gid, cancellationToken))
                {
                    await ProcessSubmittedAsync(tmTransactionModel.Gid, cancellationToken);
                }
            }
            else if (tmTransactionModel.Status == LocalTmConst.StatusSubmit)
            {
                await ProcessSubmittedAsync(tmTransactionModel.Gid, cancellationToken);
            }
        }
    }

    public virtual async Task ProcessSubmittedAsync(string gid, CancellationToken cancellationToken = default)
    {
        var tmTransactionModel = await GetAsync(gid, cancellationToken);

        if (!await IsInStatusAsync(tmTransactionModel, LocalTmConst.StatusSubmit))
        {
            return;
        }

        try
        {
            foreach (var stepInfoModel in tmTransactionModel.Steps.Steps)
            {
                await LocalTmStepExecutor.ExecuteAsync(gid, stepInfoModel, cancellationToken);
                stepInfoModel.MarkAsExecuted();
            }

            await UpdateFinishAsync(tmTransactionModel, cancellationToken);

            Logger.LogInformation("Local transaction '{gid}' completed.", gid);
        }
        catch (Exception ex)
        {
            await UpdateNextRetryAsync(tmTransactionModel, cancellationToken);

            Logger.LogWarning(ex, "Local transaction '{gid}' process submit failed. Wait for the next retry.", gid);
        }
    }

    protected virtual async Task<bool> ProcessQueryPreparedAsync(string gid, CancellationToken cancellationToken)
    {
        var tmTransactionModel = await GetAsync(gid, cancellationToken);

        if (!await IsInStatusAsync(tmTransactionModel, LocalTmConst.StatusPrepare))
        {
            return false;
        }

        try
        {
            if (await TryInsertBarrierAsRollbackAsync(tmTransactionModel, cancellationToken))
            {
                await UpdateRollbackAsync(tmTransactionModel, LocalTmConst.ReasonPrepareFailure, cancellationToken);

                Logger.LogInformation("Local transaction '{gid}' rolled back.", gid);

                return false;
            }

            await UpdateSubmitAsync(tmTransactionModel, cancellationToken);

            Logger.LogInformation("Local transaction '{gid}' committed.", gid);

            return true;
        }
        catch (Exception ex)
        {
            await UpdateNextRetryAsync(tmTransactionModel, cancellationToken);

            Logger.LogWarning(ex, "Local transaction '{gid}' process query prepared failed. Wait for the next retry.", gid);

            return false;
        }
    }

    protected virtual async Task<bool> TryInsertBarrierAsRollbackAsync(TmTransactionModel tmTransactionModel, CancellationToken cancellationToken)
    {
        var dbContextLookupInfoModel = tmTransactionModel.SteppingDbContextLookupInfo;
        var dbContextProvider = await DbContextProviderResolver.ResolveAsync(dbContextLookupInfoModel.DbProviderName);
        var dbContext = await dbContextProvider.GetAsync(dbContextLookupInfoModel);

        var barrierInfoModel = await BarrierInfoModelFactory.CreateForRollbackAsync(tmTransactionModel.Gid);

        var barrierInserter = await DbBarrierInserterResolver.ResolveAsync(dbContextLookupInfoModel.DbProviderName);

        if (await barrierInserter.TryInsertBarrierAsync(barrierInfoModel, dbContext, cancellationToken))
        {
            return true;
        }

        return false;
    }

    protected virtual Task<bool> IsInStatusAsync(TmTransactionModel tmTransactionModel, string expectedStatus)
    {
        if (tmTransactionModel.Status != expectedStatus)
        {
            Logger.LogWarning("Local transaction '{gid}' status '{status}' is not expected status {expectedStatus}.",
                tmTransactionModel.Gid,
                tmTransactionModel.Status,
                expectedStatus
            );
            return Task.FromResult(false);
        }

        return Task.FromResult(true);
    }

    protected virtual async Task<TmTransactionModel> GetAsync(string gid, CancellationToken cancellationToken)
    {
        return await TransactionStore.GetAsync(gid, cancellationToken);
    }

    protected virtual async Task CreateAsync(TmTransactionModel tmTransactionModel, CancellationToken cancellationToken)
    {
        tmTransactionModel.CreationTime = SteppingClock.Now;
        await TransactionStore.CreateAsync(tmTransactionModel, cancellationToken);
    }

    protected virtual async Task UpdateAsync(TmTransactionModel tmTransactionModel, CancellationToken cancellationToken)
    {
        tmTransactionModel.UpdateTime = SteppingClock.Now;
        await TransactionStore.UpdateAsync(tmTransactionModel, cancellationToken);
    }

    protected virtual async Task UpdateSubmitAsync(TmTransactionModel tmTransactionModel, CancellationToken cancellationToken)
    {
        tmTransactionModel.Status = LocalTmConst.StatusSubmit;
        await UpdateAsync(tmTransactionModel, cancellationToken);
    }

    protected virtual async Task UpdateNextRetryAsync(TmTransactionModel tmTransactionModel, CancellationToken cancellationToken)
    {
        tmTransactionModel.CalculateNextRetryTime();
        await UpdateAsync(tmTransactionModel, cancellationToken);
    }

    protected virtual async Task UpdateRollbackAsync(TmTransactionModel tmTransactionModel, string reason, CancellationToken cancellationToken)
    {
        tmTransactionModel.Status = LocalTmConst.StatusRollback;
        tmTransactionModel.RollbackReason = reason;
        tmTransactionModel.RollbackTime = SteppingClock.Now;
        await UpdateAsync(tmTransactionModel, cancellationToken);
    }

    protected virtual async Task UpdateFinishAsync(TmTransactionModel tmTransactionModel, CancellationToken cancellationToken)
    {
        tmTransactionModel.Status = LocalTmConst.StatusFinish;
        tmTransactionModel.FinishTime = SteppingClock.Now;
        await UpdateAsync(tmTransactionModel, cancellationToken);
    }
}
