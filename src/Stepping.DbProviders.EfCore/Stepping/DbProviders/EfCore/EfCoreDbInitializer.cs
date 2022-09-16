using System.Collections.Concurrent;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stepping.Core.Databases;
using Stepping.Core.Options;

namespace Stepping.DbProviders.EfCore;

public class EfCoreDbInitializer : IDbInitializer
{
    private ILogger<EfCoreDbInitializer> Logger { get; }

    protected static ConcurrentDictionary<string, bool> CreatedConnectionStrings { get; } = new();
    public static bool CacheEnabled { get; set; }

    protected SteppingOptions Options { get; }

    public EfCoreDbInitializer(
        ILogger<EfCoreDbInitializer> logger,
        IOptions<SteppingOptions> options)
    {
        Logger = logger;
        Options = options.Value;
    }

    public virtual async Task TryInitializeAsync(IDbInitializingInfoModel infoModel)
    {
        var dbContext = (EfCoreSteppingDbContext)infoModel.DbContext;

        var connectionString = dbContext.ConnectionString;

        if (CacheEnabled && CreatedConnectionStrings.ContainsKey(connectionString!))
        {
            return;
        }

        BarrierSqlTemplates.DbProviderSpecialMapping.TryGetValue(dbContext.DbContext.Database.ProviderName!,
            out var special);

        Logger.LogInformation("EfCoreDbInitializer found database provider: {databaseName}",
            dbContext.DbContext.Database.ProviderName);

        if (special is null)
        {
            throw new NotSupportedException(
                $"Database provider {dbContext.DbContext.Database.ProviderName} is not supported by Stepping!");
        }

        var currentTransaction = dbContext.DbContext.Database.CurrentTransaction;
        var existBarrierTableSql = special.GetExistBarrierTableSql(Options);

        var tableCount = await dbContext.DbContext.Database.GetDbConnection().QueryFirstOrDefaultAsync<int>(
            existBarrierTableSql, null, currentTransaction?.GetDbTransaction());

        if (tableCount > 0)
        {
            CreatedConnectionStrings[connectionString] = true;
            return;
        }

        var sql = special.GetCreateBarrierTableSql(Options);

        if (currentTransaction is null)
        {
            await dbContext.DbContext.Database.GetDbConnection().ExecuteAsync(sql);
        }
        else
        {
            await dbContext.DbContext.Database.GetDbConnection().ExecuteAsync(sql);
        }
    }
}