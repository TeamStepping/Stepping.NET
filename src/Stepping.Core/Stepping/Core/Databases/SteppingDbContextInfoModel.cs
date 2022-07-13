namespace Stepping.Core.Databases;

public class SteppingDbContextInfoModel
{
    public string DbProviderName { get; set; } = null!;

    public string? DbContextType { get; set; }

    public string? Database { get; set; }

    public string EncryptedConnectionString { get; set; } = null!;

    protected SteppingDbContextInfoModel()
    {
    }

    public SteppingDbContextInfoModel(
        string dbProviderName,
        string? dbContextType,
        string? database,
        string encryptedConnectionString)
    {
        DbProviderName = dbProviderName;
        DbContextType = dbContextType;
        Database = database;
        EncryptedConnectionString = encryptedConnectionString;
    }

    public static string GetTypeFullNameWithAssemblyName(Type type)
    {
        return $"{type.FullName}, {type.Assembly.GetName().Name}";
    }
}