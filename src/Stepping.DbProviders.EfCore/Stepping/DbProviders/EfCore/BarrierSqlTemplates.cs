using Stepping.DbProviders.EfCore.DbSpecials;

namespace Stepping.DbProviders.EfCore;

public static class BarrierSqlTemplates
{
    private static IDbSpecial MySqlDbSpecial { get; set; } = new MySqlDbSpecial();
    private static IDbSpecial PostgreSqlDbSpecial { get; set; } = new PostgreSqlDbSpecial();
    private static IDbSpecial SqlServerDbSpecial { get; set; } = new SqlServerDbSpecial();
    private static IDbSpecial SqLiteDbSpecial { get; set; } = new SqLiteDbSpecial();

    public static Dictionary<string, IDbSpecial> DbProviderSpecialMapping { get; } = new()
    {
        // MySQL
        { "Pomelo.EntityFrameworkCore.MySql", MySqlDbSpecial },
        { "MySql.EntityFrameworkCore", MySqlDbSpecial },
        { "Devart.Data.MySql.EFCore", MySqlDbSpecial },
        
        // PostgreSQL
        { "Npgsql.EntityFrameworkCore.PostgreSQL", PostgreSqlDbSpecial },
        { "Devart.Data.PostgreSql.EFCore", PostgreSqlDbSpecial },
        
        // SQL Server
        { "Microsoft.EntityFrameworkCore.SqlServer", SqlServerDbSpecial },
        
        // SQLite
        { "Microsoft.EntityFrameworkCore.Sqlite", SqLiteDbSpecial },
    };
}