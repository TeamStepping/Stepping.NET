using Microsoft.Extensions.DependencyInjection;
using Stepping.TestBase;

namespace Stepping.DbProviders.MongoDb.Tests;

public abstract class SteppingDbProvidersMongoTestBase : SteppingTestBase
{
    protected override void ConfigureServices(ServiceCollection services)
    {
        MongoDbInitializer.CacheEnabled = false;

        services.AddSteppingMongoDb();

        base.ConfigureServices(services);
    }
}