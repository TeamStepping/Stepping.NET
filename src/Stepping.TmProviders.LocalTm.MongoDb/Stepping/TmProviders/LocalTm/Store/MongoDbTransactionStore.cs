using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Stepping.Core.Databases;
using Stepping.Core.Exceptions;
using Stepping.Core.Infrastructures;
using Stepping.TmProviders.LocalTm.Models;
using Stepping.TmProviders.LocalTm.MongoDb;
using Stepping.TmProviders.LocalTm.Options;
using Stepping.TmProviders.LocalTm.Steps;
using Stepping.TmProviders.LocalTm.Timing;

namespace Stepping.TmProviders.LocalTm.Store;

public class MongoDbTransactionStore : ITransactionStore
{
    protected LocalTmMongoDbContext LocalTmMongoDbContext { get; }

    protected ISteppingJsonSerializer JsonSerializer { get; }

    protected LocalTmOptions Options { get; }

    protected ILocalTmMongoDbInitializer LocalTmMongoDbInitializer { get; }

    protected ISteppingClock SteppingClock { get; }

    public MongoDbTransactionStore(
        LocalTmMongoDbContext localTmMongoDbContext,
        ISteppingJsonSerializer jsonSerializer,
        IOptionsMonitor<LocalTmOptions> optionsMonitor,
        ILocalTmMongoDbInitializer localTmMongoDbInitializer,
        ISteppingClock steppingClock)
    {
        LocalTmMongoDbContext = localTmMongoDbContext;
        JsonSerializer = jsonSerializer;
        Options = optionsMonitor.CurrentValue;
        LocalTmMongoDbInitializer = localTmMongoDbInitializer;
        SteppingClock = steppingClock;
    }

    public virtual async Task<List<TmTransactionModel>> GetPendingListAsync(CancellationToken cancellationToken = default)
    {
        await LocalTmMongoDbInitializer.TryInitializeAsync();

        var now = SteppingClock.Now;
        var timeoutTime = now.Add(-Options.Timeout);

        var tmTransactions = await LocalTmMongoDbContext.GetTmTransactionCollection().AsQueryable()
            .Where(x =>
                x.Status != LocalTmConst.StatusFinish && x.Status != LocalTmConst.StatusRollback &&
                ((x.NextRetryTime == null && x.CreationTime <= timeoutTime ) || x.NextRetryTime <= now)
            )
            .OrderBy(x => x.NextRetryTime)
            .ToListAsync(cancellationToken: cancellationToken);

        return tmTransactions.ConvertAll(ConvertToModel);
    }

    public virtual async Task<TmTransactionModel> GetAsync(string gid, CancellationToken cancellationToken = default)
    {
        await LocalTmMongoDbInitializer.TryInitializeAsync();

        var document = await LocalTmMongoDbContext.GetTmTransactionCollection().AsQueryable()
            .SingleAsync(x => x.Gid == gid, cancellationToken: cancellationToken);

        return ConvertToModel(document);
    }

    public virtual async Task<TmTransactionModel?> FindAsync(string gid, CancellationToken cancellationToken = default)
    {
        await LocalTmMongoDbInitializer.TryInitializeAsync();

        var document = await LocalTmMongoDbContext.GetTmTransactionCollection().AsQueryable()
            .SingleOrDefaultAsync(x => x.Gid == gid, cancellationToken: cancellationToken);

        return document != null ? ConvertToModel(document) : null;
    }

    public virtual async Task CreateAsync(TmTransactionModel tmTransaction, CancellationToken cancellationToken = default)
    {
        await LocalTmMongoDbInitializer.TryInitializeAsync();

        var document = ConvertToDocument(tmTransaction);

        SetNewConcurrencyStamp(document);

        await LocalTmMongoDbContext.GetTmTransactionCollection()
            .InsertOneAsync(document, cancellationToken: cancellationToken);
    }

    public virtual async Task UpdateAsync(TmTransactionModel tmTransaction, CancellationToken cancellationToken = default)
    {
        await LocalTmMongoDbInitializer.TryInitializeAsync();

        var document = ConvertToDocument(tmTransaction);

        var oldConcurrencyStamp = SetNewConcurrencyStamp(document);

        var result = await LocalTmMongoDbContext.GetTmTransactionCollection()
            .ReplaceOneAsync(
                x => x.Gid == tmTransaction.Gid && x.ConcurrencyStamp == oldConcurrencyStamp,
                document,
                new ReplaceOptions() { IsUpsert = false },
                cancellationToken: cancellationToken
            );

        if (result.ModifiedCount == 0)
        {
            throw new SteppingException($"Local transaction {tmTransaction.ConcurrencyStamp} concurrency stamp is not match!");
        }
    }

    protected virtual TmTransactionDocument ConvertToDocument(TmTransactionModel model)
    {
        return new TmTransactionDocument()
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
            SteppingDbContextLookupInfo = model.SteppingDbContextLookupInfo != null ? JsonSerializer.Serialize(model.SteppingDbContextLookupInfo) : null,
            ConcurrencyStamp = model.ConcurrencyStamp,
        };
    }

    protected virtual TmTransactionModel ConvertToModel(TmTransactionDocument document)
    {
        var steps = JsonSerializer.Deserialize<LocalTmStepModel>(document.Steps);
        var steppingDbContextLookupInfo = document.SteppingDbContextLookupInfo != null ? JsonSerializer.Deserialize<SteppingDbContextLookupInfoModel>(document.SteppingDbContextLookupInfo) : null;

        return new TmTransactionModel(document.Gid, steps, steppingDbContextLookupInfo, document.CreationTime)
        {
            Status = document.Status,
            CreationTime = document.CreationTime,
            UpdateTime = document.UpdateTime,
            FinishTime = document.FinishTime,
            RollbackReason = document.RollbackReason,
            RollbackTime = document.RollbackTime,
            NextRetryInterval = document.NextRetryInterval,
            NextRetryTime = document.NextRetryTime,
            ConcurrencyStamp = document.ConcurrencyStamp
        };
    }

    protected virtual string? SetNewConcurrencyStamp(TmTransactionDocument document)
    {
        var oldConcurrencyStamp = document.ConcurrencyStamp;
        document.ConcurrencyStamp = Guid.NewGuid().ToString("N");
        return oldConcurrencyStamp;
    }
}
