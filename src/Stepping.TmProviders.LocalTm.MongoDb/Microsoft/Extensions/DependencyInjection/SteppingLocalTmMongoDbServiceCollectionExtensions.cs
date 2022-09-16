using Stepping.TmProviders.LocalTm.MongoDb;
using Stepping.TmProviders.LocalTm.Store;

namespace Microsoft.Extensions.DependencyInjection;

public static class SteppingLocalTmMongoDbServiceCollectionExtensions
{
    public static IServiceCollection AddSteppingLocalTmMongoDb(this IServiceCollection services, Action<LocalTmMongoDbOptions> setupAction)
    {
        services.Configure(setupAction);

        services.AddSingleton<LocalTmMongoDbContext>();

        services.AddTransient<MongoDbTransactionStore>();
        services.AddTransient<ITransactionStore, MongoDbTransactionStore>();

        services.AddTransient<LocalTmMongoDbInitializer>();
        services.AddTransient<ILocalTmMongoDbInitializer, LocalTmMongoDbInitializer>();

        return services;
    }
}
