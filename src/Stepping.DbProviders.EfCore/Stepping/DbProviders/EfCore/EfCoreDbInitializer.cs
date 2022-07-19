using System.Collections.Concurrent;
using System.Data;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stepping.Core.Databases;
using Stepping.Core.Extensions;
using Stepping.Core.Infrastructures;
using Stepping.Core.Options;

namespace Stepping.DbProviders.EfCore;

public class EfCoreDbInitializer : IDbInitializer
{
    private ILogger<EfCoreDbInitializer> Logger { get; }

    protected static ConcurrentDictionary<string, bool> CreatedConnectionStrings { get; } = new();
    public static bool CacheEnabled { get; set; }

    protected SteppingOptions Options { get; }
    protected IServiceProvider ServiceProvider { get; }

    public EfCoreDbInitializer(
        ILogger<EfCoreDbInitializer> logger,
        IOptions<SteppingOptions> options,
        IServiceProvider serviceProvider)
    {
        Logger = logger;
        Options = options.Value;
        ServiceProvider = serviceProvider;
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

        // Todo: Check table exists. If so, we cache it in CreatedConnectionStrings. So we don't need to create a new scope and also support cache for IsolationLevel.Serializable.
        // var sql = special.GetExistBarrierTableSql(Options);

        var sql = special.GetCreateBarrierTableSql(Options);

        var currentTransaction = dbContext.DbContext.Database.CurrentTransaction;

        if (currentTransaction is null)
        {
            await dbContext.DbContext.Database.GetDbConnection().ExecuteAsync(sql);
            CreatedConnectionStrings[connectionString] = true;
        }
        else if (currentTransaction.GetDbTransaction().IsolationLevel == IsolationLevel.Serializable)
        {
            await dbContext.DbContext.Database.GetDbConnection().ExecuteAsync(sql);
            // Don't set a item in CreatedConnectionStrings
        }
        else
        {
            await using var scope = ServiceProvider.CreateAsyncScope();

            var newSteppingDbContext = await GetNewSteppingDbContextAsync(scope.ServiceProvider, dbContext);

            await newSteppingDbContext.DbContext.Database.GetDbConnection().ExecuteAsync(sql);
            CreatedConnectionStrings[connectionString] = true;
        }
    }

    protected virtual async Task<EfCoreSteppingDbContext> GetNewSteppingDbContextAsync(
        IServiceProvider serviceProvider, EfCoreSteppingDbContext originalDbContext)
    {
        var connectionStringHasher = serviceProvider.GetRequiredService<IConnectionStringHasher>();
        var dbContextProviderResolver = serviceProvider.GetRequiredService<ISteppingDbContextProviderResolver>();
        var tenantIdProvider = serviceProvider.GetRequiredService<ISteppingTenantIdProvider>();

        var dbContextProvider =
            await dbContextProviderResolver.ResolveAsync(SteppingDbProviderEfCoreConsts.DbProviderName);

        var connectionString = originalDbContext.DbContext.Database.GetConnectionString() ??
                               throw new InvalidOperationException();

        return (EfCoreSteppingDbContext)await dbContextProvider.GetAsync(
            new SteppingDbContextLookupInfoModel(
                SteppingDbProviderEfCoreConsts.DbProviderName,
                await connectionStringHasher.HashAsync(connectionString),
                originalDbContext.GetType().GetTypeFullNameWithAssemblyName(),
                null,
                await tenantIdProvider.GetCurrentAsync(),
                originalDbContext.CustomInfo
            )
        );
    }
}