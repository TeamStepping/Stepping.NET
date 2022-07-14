using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Stepping.Core;
using Stepping.Core.Databases;
using Stepping.Core.Exceptions;

namespace Stepping.DbProviders.MongoDb;

public class MongoDbBarrierInserter : IDbBarrierInserter
{
    public string DbProviderName => SteppingDbProviderMongoDbConsts.DbProviderName;

    private ILogger<MongoDbBarrierInserter> Logger { get; }
    protected IBarrierCollectionProvider BarrierCollectionProvider { get; }
    protected MongoDbInitializer DbInitializer { get; }

    public MongoDbBarrierInserter(
        ILogger<MongoDbBarrierInserter> logger,
        IBarrierCollectionProvider barrierCollectionProvider,
        MongoDbInitializer dbInitializer)
    {
        Logger = logger;
        BarrierCollectionProvider = barrierCollectionProvider;
        DbInitializer = dbInitializer;
    }

    public virtual async Task MustInsertBarrierAsync(BarrierInfoModel barrierInfoModel, ISteppingDbContext dbContext,
        CancellationToken cancellationToken = default)
    {
        var mongoDbContext = (MongoDbSteppingDbContext)dbContext;

        var mongoCollection = await BarrierCollectionProvider.GetAsync(mongoDbContext);

        await DbInitializer.TryInitializeAsync(new MongoDbInitializingInfoModel(mongoDbContext));

        try
        {
            var document = new SteppingBarrierDocument(
                transType: barrierInfoModel.TransType,
                gid: barrierInfoModel.Gid,
                branchId: barrierInfoModel.BranchId,
                op: barrierInfoModel.Op,
                barrierId: barrierInfoModel.BarrierId,
                reason: barrierInfoModel.Reason);

            if (mongoDbContext.SessionHandle is null)
            {
                await mongoCollection.InsertOneAsync(
                    document,
                    cancellationToken: cancellationToken);
            }
            else
            {
                await mongoCollection.InsertOneAsync(
                    mongoDbContext.SessionHandle,
                    document,
                    cancellationToken: cancellationToken);
            }
        }
        catch (Exception e)
        {
            if (e is MongoWriteException && e.Message.Contains("DuplicateKey"))
            {
                Logger.LogDebug(
                    "Barrier exists, transType={transType}, gid={gid}, branchId={branchId}, op={op}, barrierId={barrierId}",
                    barrierInfoModel.Gid, barrierInfoModel.TransType, barrierInfoModel.BranchId, barrierInfoModel.Op,
                    barrierInfoModel.BarrierId);

                throw new DuplicateBarrierException();
            }

            throw;
        }
    }

    public virtual async Task<bool> TryInsertBarrierAsync(BarrierInfoModel barrierInfoModel,
        ISteppingDbContext dbContext, CancellationToken cancellationToken = default)
    {
        var mongoDbContext = (MongoDbSteppingDbContext)dbContext;

        try
        {
            await MustInsertBarrierAsync(barrierInfoModel, dbContext, cancellationToken);
        }
        catch (Exception e)
        {
            Logger.LogWarning(e, "Insert Barrier error, gid={gid}", barrierInfoModel.Gid);

            if (e is not DuplicateBarrierException)
            {
                throw;
            }
        }

        try
        {
            var mongoCollection = await BarrierCollectionProvider.GetAsync(mongoDbContext);

            var filter = BuildFindFilters(barrierInfoModel.Gid, barrierInfoModel.BranchId, barrierInfoModel.TransType,
                barrierInfoModel.BarrierId);

            var cursor = mongoDbContext.SessionHandle is null
                ? await mongoCollection.FindAsync<SteppingBarrierDocument>(filter, cancellationToken: cancellationToken)
                : await mongoCollection.FindAsync<SteppingBarrierDocument>(mongoDbContext.SessionHandle, filter,
                    cancellationToken: cancellationToken);

            var res = await cursor.ToListAsync(cancellationToken: cancellationToken);

            if (res is { Count: > 0 } && res[0].Reason.Equals(barrierInfoModel.Reason))
            {
                return true;
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Query Prepared error, gid={gid}", barrierInfoModel.Gid);
            throw;
        }

        return false;
    }

    protected virtual FilterDefinition<SteppingBarrierDocument> BuildFindFilters(
        string gid, string branchId, string op, string barrierId)
    {
        return new FilterDefinitionBuilder<SteppingBarrierDocument>().And(
            Builders<SteppingBarrierDocument>.Filter.Eq(x => x.Gid, gid),
            Builders<SteppingBarrierDocument>.Filter.Eq(x => x.BranchId, branchId),
            Builders<SteppingBarrierDocument>.Filter.Eq(x => x.Op, op),
            Builders<SteppingBarrierDocument>.Filter.Eq(x => x.BarrierId, barrierId)
        );
    }
}