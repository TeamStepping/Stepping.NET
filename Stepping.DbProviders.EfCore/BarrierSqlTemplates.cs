using Stepping.DbProviders.EfCore.DbSpecials;

namespace Stepping.DbProviders.EfCore;

public static class BarrierSqlTemplates
{
    private static IDbSpecial MySQLDbSpecial { get; set; } = new MySqlDbSpecial();
    private static IDbSpecial PostgreSQLDbSpecial { get; set; } = new PostgreSqlDbSpecial();
    private static IDbSpecial SQLServerDbSpecial { get; set; } = new SqlServerDbSpecial();
    private static IDbSpecial SQLiteDbSpecial { get; set; } = new SqLiteDbSpecial();

    public static Dictionary<string, IDbSpecial> DbProviderSpecialMapping { get; } = new()
    {
        // MySQL
        { "Pomelo.EntityFrameworkCore.MySql", MySQLDbSpecial },
        { "MySql.EntityFrameworkCore", MySQLDbSpecial },
        { "Devart.Data.MySql.EFCore", MySQLDbSpecial },
        
        // PostgreSQL
        { "Npgsql.EntityFrameworkCore.PostgreSQL", PostgreSQLDbSpecial },
        { "Devart.Data.PostgreSql.EFCore", PostgreSQLDbSpecial },
        
        // SQL Server
        { "Microsoft.EntityFrameworkCore.SqlServer", SQLServerDbSpecial },
        
        // SQLite
        { "Microsoft.EntityFrameworkCore.Sqlite", SQLiteDbSpecial },
    };
}