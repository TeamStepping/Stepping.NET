namespace Stepping.TmProviders.Dtm.Grpc;

public static class DtmRequestHeaderNames
{
    public static string DtmGid { get; set; } = "dtm-gid";
    public static string ActionApiToken { get; set; } = "ActionApiToken";
    public static string DbProviderName { get; set; } = "DbProviderName";
    public static string DbContextType { get; set; } = "DbContextType";
    public static string Database { get; set; } = "Database";
    public static string EncryptedConnectionString { get; set; } = "EncryptedConnectionString";
}