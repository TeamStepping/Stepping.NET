using Microsoft.Extensions.DependencyInjection;
using Stepping.TestBase;
using Stepping.TmProviders.LocalTm.MongoDb;
using Stepping.TmProviders.LocalTm.MongoDb.Tests;

namespace Stepping.TmProviders.LocalTm.Core.Tests;

public class SteppingTmProvidersLocalTmMongoDbTestBase : SteppingTestBase
{
    protected override void ConfigureServices(ServiceCollection services)
    {
        LocalTmMongoDbInitializer.CacheEnabled = false;

        services.AddSteppingLocalTm();

        services.AddSteppingLocalTmMongoDb(options =>
        {
            options.ConnectionString = MongoDbFixture.ConnectionString;
            options.DatabaseName = MongoDbTestConsts.Database;
        });

        base.ConfigureServices(services);
    }
}