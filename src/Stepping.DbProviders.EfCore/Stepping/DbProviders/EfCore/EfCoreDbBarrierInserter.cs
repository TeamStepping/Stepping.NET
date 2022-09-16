using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stepping.Core;
using Stepping.Core.Databases;
using Stepping.Core.Exceptions;
using Stepping.Core.Options;

namespace Stepping.DbProviders.EfCore;

public class EfCoreDbBarrierInserter : IDbBarrierInserter
{
    public string DbProviderName => SteppingDbProviderEfCoreConsts.DbProviderName;

    private ILogger<EfCoreDbBarrierInserter> Logger { get; }
    protected SteppingOptions Options { get; }
    protected EfCoreDbInitializer DbInitializer { get; }

    public EfCoreDbBarrierInserter(
        ILogger<EfCoreDbBarrierInserter> logger,
        IOptions<SteppingOptions> options,
        EfCoreDbInitializer dbInitializer)
    {
        Logger = logger;
        Options = options.Value;
        DbInitializer = dbInitializer;
    }

    public virtual async Task MustInsertBarrierAsync(BarrierInfoModel barrierInfoModel, ISteppingDbContext dbContext,
        CancellationToken cancellationToken = default)
    {
        var affected =
            await InsertBarrierAsync(barrierInfoModel, (EfCoreSteppingDbContext)dbContext, cancellationToken);

        Logger.LogDebug("currentAffected: {currentAffected}", affected);

        if (affected == 0)
        {
            Logger.LogDebug(
                "Barrier exists, transType={transType}, gid={gid}, branchId={branchId}, op={op}, barrierId={barrierId}",
                barrierInfoModel.Gid, barrierInfoModel.TransType, barrierInfoModel.BranchId, barrierInfoModel.Op,
                barrierInfoModel.BarrierId);

            throw new DuplicateBarrierException();
        }
    }

    public virtual async Task<bool> TryInsertBarrierAsync(BarrierInfoModel barrierInfoModel,
        ISteppingDbContext dbContext, CancellationToken cancellationToken = default)
    {
        try
        {
            await InsertBarrierAsync(barrierInfoModel, (EfCoreSteppingDbContext)dbContext, cancellationToken);
        }
        catch (Exception e)
        {
            Logger.LogWarning(e, "Insert Barrier error, gid={gid}", barrierInfoModel.Gid);
            throw;
        }

        var efCoreDbContext = (EfCoreSteppingDbContext)dbContext;

        try
        {
            BarrierSqlTemplates.DbProviderSpecialMapping.TryGetValue(efCoreDbContext.DbContext.Database.ProviderName!,
                out var special);

            var reason = await efCoreDbContext.DbContext.Database.GetDbConnection().QueryFirstAsync<string>(
                special!.GetQueryPreparedSql(Options.BarrierTableName),
                new
                {
                    gid = barrierInfoModel.Gid,
                    branch_id = barrierInfoModel.BranchId,
                    op = barrierInfoModel.Op,
                    barrier_id = barrierInfoModel.BarrierId
                },
                efCoreDbContext.DbContext.Database.CurrentTransaction?.GetDbTransaction());

            if (reason.Equals(barrierInfoModel.Reason))
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

    protected virtual async Task<int> InsertBarrierAsync(BarrierInfoModel barrierInfoModel,
        EfCoreSteppingDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await DbInitializer.TryInitializeAsync(new EfCoreDbInitializingInfoModel(dbContext));

        BarrierSqlTemplates.DbProviderSpecialMapping.TryGetValue(dbContext.DbContext.Database.ProviderName!,
            out var special);

        if (special is null)
        {
            throw new NotSupportedException(
                $"Database provider {dbContext.DbContext.Database.ProviderName} is not supported by the event boxes!");
        }

        var sql = special.GetInsertIgnoreSqlTemplate(Options.BarrierTableName);

        var affected = await dbContext.DbContext.Database.GetDbConnection().ExecuteAsync(
            sql,
            new
            {
                // Reuse the design of DTM.
                trans_type = barrierInfoModel.TransType,
                gid = barrierInfoModel.Gid,
                branch_id = barrierInfoModel.BranchId,
                op = barrierInfoModel.Op,
                barrier_id = barrierInfoModel.BarrierId,
                reason = barrierInfoModel.Reason
            },
            dbContext.DbContext.Database.CurrentTransaction?.GetDbTransaction());

        return affected;
    }
}