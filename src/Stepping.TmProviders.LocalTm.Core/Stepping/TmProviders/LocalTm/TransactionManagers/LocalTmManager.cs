using Microsoft.Extensions.Logging;
using Stepping.Core.Databases;
using Stepping.Core.Exceptions;
using Stepping.Core.Infrastructures;
using Stepping.Core.Steps;
using Stepping.TmProviders.LocalTm.Models;
using Stepping.TmProviders.LocalTm.Steps;
using Stepping.TmProviders.LocalTm.Store;
using Stepping.TmProviders.LocalTm.Timing;

namespace Stepping.TmProviders.LocalTm.TransactionManagers;

public class LocalTmManager : ILocalTmManager
{
    protected ITransactionStore TransactionStore { get; }

    protected ISteppingDbContextProviderResolver DbContextProviderResolver { get; }

    protected IBarrierInfoModelFactory BarrierInfoModelFactory { get; }

    protected IDbBarrierInserterResolver DbBarrierInserterResolver { get; }

    protected ILogger<LocalTmManager> Logger { get; }

    protected ISteppingClock SteppingClock { get; }

    protected IStepResolver StepResolver { get; }

    protected IStepExecutor StepExecutor { get; }

    protected ISteppingJsonSerializer SteppingJsonSerializer { get; }

    protected IHttpClientFactory HttpClientFactory { get; }

    public LocalTmManager(
        ITransactionStore transactionStore,
        ISteppingDbContextProviderResolver dbContextProviderResolver,
        IBarrierInfoModelFactory barrierInfoModelFactory,
        IDbBarrierInserterResolver dbBarrierInserterResolver,
        ILogger<LocalTmManager> logger,
        ISteppingClock steppingClock,
        IStepResolver stepResolver,
        IStepExecutor stepExecutor,
        ISteppingJsonSerializer steppingJsonSerializer,
        IHttpClientFactory httpClientFactory)
    {
        TransactionStore = transactionStore;
        DbContextProviderResolver = dbContextProviderResolver;
        BarrierInfoModelFactory = barrierInfoModelFactory;
        DbBarrierInserterResolver = dbBarrierInserterResolver;
        Logger = logger;
        SteppingClock = steppingClock;
        StepResolver = stepResolver;
        StepExecutor = stepExecutor;
        SteppingJsonSerializer = steppingJsonSerializer;
        HttpClientFactory = httpClientFactory;
    }

    public virtual async Task PrepareAsync(string gid, LocalTmStepModel steps, SteppingDbContextLookupInfoModel steppingDbContextLookupInfo,
        CancellationToken cancellationToken = default)
    {
        var tmTransactionModel = new TmTransactionModel(gid, steps, steppingDbContextLookupInfo, SteppingClock.Now);

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
            foreach (var stepInfoModel in tmTransactionModel.Steps.Steps.Where(x => !x.Executed))
            {
                await ExecuteStepsAsync(gid, stepInfoModel, cancellationToken);
                stepInfoModel.MarkAsExecuted();
            }

            await UpdateFinishAsync(tmTransactionModel, cancellationToken);

            Logger.LogInformation("Local transaction '{gid}' completed.", gid);
        }
        catch (Exception ex)
        {
            await UpdateNextRetryAsync(tmTransactionModel, cancellationToken);

            Logger.LogWarning(ex, "Local transaction '{gid}' process submitted failed. Wait for the next retry.", gid);
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

    protected virtual async Task ExecuteStepsAsync(string gid, LocalTmStepInfoModel stepInfoModel, CancellationToken cancellationToken = default)
    {
        object? args = null;

        if (!string.IsNullOrWhiteSpace(stepInfoModel.ArgsToByteString))
        {
            args = await StepResolver.ResolveArgsAsync(stepInfoModel.StepName, stepInfoModel.ArgsToByteString);
        }

        var step = StepResolver.Resolve(stepInfoModel.StepName, args);

        if (step is IExecutableStep)
        {
            await StepExecutor.ExecuteAsync(gid, stepInfoModel.StepName, stepInfoModel.ArgsToByteString, cancellationToken);
            return;
        }

        if (step is HttpRequestStep httpRequestStep)
        {
            var response = await HttpClientFactory.CreateClient(LocalTmConst.LocalTmHttpClient)
                .SendAsync(httpRequestStep.CreateHttpRequestMessage(SteppingJsonSerializer), cancellationToken);
            response.EnsureSuccessStatusCode();
            return;
        }

        throw new SteppingException($"Unknown step type: {step.GetType().FullName}.");
    }

    #region Store

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
        tmTransactionModel.CalculateNextRetryTime(SteppingClock.Now);
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

    #endregion
}
