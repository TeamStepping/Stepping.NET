namespace Stepping.Core.Databases;

public class SteppingDbContextLookupInfoModel
{
    public string DbProviderName { get; set; } = null!;

    public string HashedConnectionString { get; set; } = null!;

    public string? DbContextType { get; set; }

    public string? Database { get; set; }

    public string? TenantId { get; set; }

    public string? CustomInfo { get; set; }

    protected SteppingDbContextLookupInfoModel()
    {
    }

    public SteppingDbContextLookupInfoModel(
        string dbProviderName,
        string hashedConnectionString,
        string? dbContextType,
        string? database,
        string? tenantId,
        string? customInfo = null)
    {
        DbProviderName = dbProviderName;
        HashedConnectionString = hashedConnectionString;
        DbContextType = dbContextType;
        Database = database;
        TenantId = tenantId;
        CustomInfo = customInfo;
    }
}