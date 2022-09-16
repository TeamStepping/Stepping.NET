using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Stepping.TestBase;

namespace Stepping.TmProviders.LocalTm.EfCore.Tests;

public class SteppingTmProvidersLocalTmEfCoreTestBase : SteppingTestBase
{
    protected override void ConfigureServices(ServiceCollection services)
    {
        services.AddSteppingLocalTm();

        var sqliteConnection = CreateDatabaseAndGetConnection();
        services.AddSteppingLocalTmEfCore(builder =>
        {
            builder.UseSqlite(sqliteConnection);
        });

        base.ConfigureServices(services);
    }

    private static SqliteConnection CreateDatabaseAndGetConnection()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<LocalTmDbContext>()
            .UseSqlite(connection)
            .Options;

        using (var context = new LocalTmDbContext(options))
        {
            context.GetService<IRelationalDatabaseCreator>().CreateTables();
        }

        return connection;
    }
}