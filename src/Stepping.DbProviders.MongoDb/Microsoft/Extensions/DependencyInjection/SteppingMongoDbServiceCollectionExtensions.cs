using Microsoft.Extensions.DependencyInjection.Extensions;
using Stepping.Core.Databases;
using Stepping.Core.Options;
using Stepping.DbProviders.MongoDb;

namespace Microsoft.Extensions.DependencyInjection;

public static class SteppingMongoDbServiceCollectionExtensions
{
    public static IServiceCollection AddSteppingMongoDb(this IServiceCollection services,
        Action<SteppingMongoDbOptions> setupAction)
    {
        services.AddSteppingMongoDbServices();

        services.Configure<SteppingOptions>(options =>
        {
            options.RegisterDbBarrierInserters(typeof(MongoDbBarrierInserter));
        });

        services.Configure(setupAction);

        return services;
    }

    private static IServiceCollection AddSteppingMongoDbServices(this IServiceCollection services)
    {
        services.TryAddTransient<IBarrierCollectionProvider, BarrierCollectionProvider>();
        services.TryAddTransient<BarrierCollectionProvider>();

        services.AddTransient<IDbBarrierInserter, MongoDbBarrierInserter>();
        services.TryAddTransient<MongoDbBarrierInserter>();

        services.AddTransient<IDbInitializer, MongoDbInitializer>();
        services.TryAddTransient<MongoDbInitializer>();

        services.AddTransient<ISteppingDbContextProvider, DefaultMongoDbSteppingDbContextProvider>();
        services.TryAddTransient<DefaultMongoDbSteppingDbContextProvider>();

        return services;
    }
}