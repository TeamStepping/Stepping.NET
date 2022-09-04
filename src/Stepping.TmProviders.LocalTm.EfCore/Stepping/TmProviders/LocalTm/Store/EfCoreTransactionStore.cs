using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Stepping.Core.Databases;
using Stepping.Core.Infrastructures;
using Stepping.TmProviders.LocalTm.EfCore;
using Stepping.TmProviders.LocalTm.Models;
using Stepping.TmProviders.LocalTm.Options;
using Stepping.TmProviders.LocalTm.Steps;
using Stepping.TmProviders.LocalTm.Timing;

namespace Stepping.TmProviders.LocalTm.Store;

public class EfCoreTransactionStore : ITransactionStore
{
    protected LocalTmDbContext LocalTmDbContext { get; }

    protected ISteppingJsonSerializer JsonSerializer { get; }

    protected LocalTmOptions Options { get; }

    protected ISteppingClock SteppingClock { get; }

    public EfCoreTransactionStore(
        LocalTmDbContext localTmDbContext,
        ISteppingJsonSerializer jsonSerializer,
        IOptionsMonitor<LocalTmOptions> optionsMonitor,
        ISteppingClock steppingClock)
    {
        LocalTmDbContext = localTmDbContext;
        JsonSerializer = jsonSerializer;
        Options = optionsMonitor.CurrentValue;
        SteppingClock = steppingClock;
    }

    public virtual async Task<List<TmTransactionModel>> GetPendingListAsync(CancellationToken cancellationToken = default)
    {
        var timeoutTime = SteppingClock.Now.Add(-Options.Timeout);

        var tmTransactions = await LocalTmDbContext.TmTransactions.AsNoTracking()
            .Where(x =>
                x.Status != LocalTmConst.StatusFinish && x.Status != LocalTmConst.StatusRollback &&
                (x.NextRetryTime == null || x.NextRetryTime >= timeoutTime) &&
                x.CreationTime >= timeoutTime
            )
            .OrderBy(x => x.NextRetryTime)
            .ToListAsync(cancellationToken);

        return tmTransactions.ConvertAll(ConvertToModel);
    }

    public virtual async Task<TmTransactionModel> GetAsync(string gid, CancellationToken cancellationToken = default)
    {
        var entity = await LocalTmDbContext.TmTransactions.AsNoTracking()
            .SingleAsync(x => x.Gid == gid, cancellationToken);

        return ConvertToModel(entity);
    }

    public virtual async Task CreateAsync(TmTransactionModel tmTransaction, CancellationToken cancellationToken = default)
    {
        var entity = ConvertToEntity(tmTransaction);

        await LocalTmDbContext.AddAsync(entity, cancellationToken);

        await LocalTmDbContext.SaveChangesAsync(cancellationToken);
    }

    public virtual async Task UpdateAsync(TmTransactionModel tmTransaction, CancellationToken cancellationToken = default)
    {
        var entity = await LocalTmDbContext.TmTransactions
            .SingleAsync(x => x.Gid == tmTransaction.Gid, cancellationToken);

        ConvertToEntity(tmTransaction, entity);

        await LocalTmDbContext.SaveChangesAsync(cancellationToken);
    }

    protected virtual TmTransaction ConvertToEntity(TmTransactionModel model)
    {
        return new TmTransaction()
        {
            Gid = model.Gid,
            Status = model.Status,
            Steps = JsonSerializer.Serialize(model.Steps),
            CreationTime = model.CreationTime,
            UpdateTime = model.UpdateTime,
            FinishTime = model.FinishTime,
            RollbackReason = model.RollbackReason,
            RollbackTime = model.RollbackTime,
            NextRetryInterval = model.NextRetryInterval,
            NextRetryTime = model.NextRetryTime,
            SteppingDbContextLookupInfo = JsonSerializer.Serialize(model.SteppingDbContextLookupInfo),
            ConcurrencyStamp = model.ConcurrencyStamp,
        };
    }

    protected virtual TmTransaction ConvertToEntity(TmTransactionModel model, TmTransaction entity)
    {
        entity.Gid = model.Gid;
        entity.Status = model.Status;
        entity.Steps = JsonSerializer.Serialize(model.Steps);
        entity.CreationTime = model.CreationTime;
        entity.UpdateTime = model.UpdateTime;
        entity.FinishTime = model.FinishTime;
        entity.RollbackReason = model.RollbackReason;
        entity.RollbackTime = model.RollbackTime;
        entity.NextRetryInterval = model.NextRetryInterval;
        entity.NextRetryTime = model.NextRetryTime;
        entity.SteppingDbContextLookupInfo = JsonSerializer.Serialize(model.SteppingDbContextLookupInfo);
        entity.ConcurrencyStamp = model.ConcurrencyStamp;

        return entity;
    }

    protected virtual TmTransactionModel ConvertToModel(TmTransaction entity)
    {
        var steps = JsonSerializer.Deserialize<LocalTmStepModel>(entity.Steps);
        var steppingDbContextLookupInfo = JsonSerializer.Deserialize<SteppingDbContextLookupInfoModel>(entity.SteppingDbContextLookupInfo);

        return new TmTransactionModel(entity.Gid, steps, steppingDbContextLookupInfo)
        {
            Status = entity.Status,
            CreationTime = entity.CreationTime,
            UpdateTime = entity.UpdateTime,
            FinishTime = entity.FinishTime,
            RollbackReason = entity.RollbackReason,
            RollbackTime = entity.RollbackTime,
            NextRetryInterval = entity.NextRetryInterval,
            NextRetryTime = entity.NextRetryTime,
            ConcurrencyStamp = entity.ConcurrencyStamp
        };
    }
}
