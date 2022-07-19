using Stepping.Core.Infrastructures;

namespace Stepping.Core.Databases;

public class SteppingDbContextLookupInfoProvider : ISteppingDbContextLookupInfoProvider
{
    protected IConnectionStringHasher ConnectionStringHasher { get; }
    protected ISteppingTenantIdProvider TenantIdProvider { get; }

    public SteppingDbContextLookupInfoProvider(
        IConnectionStringHasher connectionStringHasher,
        ISteppingTenantIdProvider tenantIdProvider)
    {
        ConnectionStringHasher = connectionStringHasher;
        TenantIdProvider = tenantIdProvider;
    }

    public virtual async Task<SteppingDbContextLookupInfoModel> GetAsync(ISteppingDbContext dbContext)
    {
        var dbProviderName = dbContext!.DbProviderName;
        var hashedConnectionString = await ConnectionStringHasher.HashAsync(dbContext.ConnectionString);
        var dbContextType = dbContext.GetInternalDbContextTypeOrNull();
        var dbContextTypeName = dbContextType is null
            ? null
            : $"{dbContextType.FullName}, {dbContextType.Assembly.GetName().Name}";

        var databaseName = dbContext.GetInternalDatabaseNameOrNull();
        var tenantId = await TenantIdProvider.GetCurrentAsync();
        var customInfo = dbContext.CustomInfo;

        return new SteppingDbContextLookupInfoModel(
            dbProviderName, hashedConnectionString, dbContextTypeName, databaseName, tenantId, customInfo);
    }
}