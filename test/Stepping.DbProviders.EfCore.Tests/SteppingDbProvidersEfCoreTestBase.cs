using Microsoft.Extensions.DependencyInjection;
using Stepping.DbProviders.EfCore.Tests.Fakes;
using Stepping.TestBase;

namespace Stepping.DbProviders.EfCore.Tests;

public abstract class SteppingDbProvidersEfCoreTestBase : SteppingTestBase
{
    protected override void ConfigureServices(ServiceCollection services)
    {
        EfCoreDbInitializer.CacheEnabled = false;

        services.AddStepping();
        services.AddSteppingEfCore();
        services.AddEntityFrameworkSqlite();

        services.AddDbContext<FakeDbContext>();
        services.AddDbContext<FakeSharedDbContext>();

        base.ConfigureServices(services);
    }
}