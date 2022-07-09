using System.Collections.Concurrent;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stepping.Core;

namespace Stepping.DbProviders.EfCore;

public class EfCoreDbInitializer : IDbInitializer
{
    private ILogger<EfCoreDbInitializer> Logger { get; }
    private ConcurrentDictionary<string, bool> CreatedConnectionStrings { get; } = new();

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

        if (CreatedConnectionStrings.ContainsKey(connectionString!))
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

        var sql = special.GetCreateBarrierTableSql(Options);

        await dbContext.DbContext.Database.GetDbConnection().ExecuteAsync(sql);

        CreatedConnectionStrings[connectionString] = true;
    }
}