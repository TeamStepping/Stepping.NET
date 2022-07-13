using Microsoft.Extensions.DependencyInjection.Extensions;
using Stepping.Core.Databases;
using Stepping.DbProviders.EfCore;

namespace Microsoft.Extensions.DependencyInjection;

public static class SteppingEfCoreServiceCollectionExtensions
{
    public static IServiceCollection AddSteppingEfCore(this IServiceCollection services)
    {
        services.AddSteppingEfCoreServices();

        return services;
    }

    private static IServiceCollection AddSteppingEfCoreServices(this IServiceCollection services)
    {
        services.AddTransient<IDbBarrierInserter, EfCoreDbBarrierInserter>();
        services.TryAddTransient<EfCoreDbBarrierInserter>();

        services.AddTransient<IDbInitializer, EfCoreDbInitializer>();
        services.TryAddTransient<EfCoreDbInitializer>();

        services.AddTransient<ISteppingDbContextProvider, EfCoreSteppingDbContextProvider>();
        services.TryAddTransient<EfCoreSteppingDbContextProvider>();

        return services;
    }
}