using Microsoft.Extensions.DependencyInjection.Extensions;
using Stepping.Core.Databases;
using Stepping.Core.Options;
using Stepping.DbProviders.EfCore;

namespace Microsoft.Extensions.DependencyInjection;

public static class SteppingEfCoreServiceCollectionExtensions
{
    public static IServiceCollection AddSteppingEfCore(this IServiceCollection services,
        Action<SteppingEfCoreOptions> setupAction)
    {
        services.AddSteppingEfCoreServices();

        services.Configure<SteppingOptions>(options =>
        {
            options.RegisterDbBarrierInserters(typeof(EfCoreDbBarrierInserter));
        });

        services.Configure(setupAction);

        return services;
    }

    public static IServiceCollection AddSteppingEfCore(this IServiceCollection services)
    {
        services.AddSteppingEfCore(_ => { });

        return services;
    }

    private static IServiceCollection AddSteppingEfCoreServices(this IServiceCollection services)
    {
        services.AddTransient<IDbBarrierInserter, EfCoreDbBarrierInserter>();
        services.TryAddTransient<EfCoreDbBarrierInserter>();

        services.AddTransient<IDbInitializer, EfCoreDbInitializer>();
        services.TryAddTransient<EfCoreDbInitializer>();

        services.AddTransient<ISteppingDbContextProvider, DefaultEfCoreSteppingDbContextProvider>();
        services.TryAddTransient<DefaultEfCoreSteppingDbContextProvider>();

        return services;
    }
}