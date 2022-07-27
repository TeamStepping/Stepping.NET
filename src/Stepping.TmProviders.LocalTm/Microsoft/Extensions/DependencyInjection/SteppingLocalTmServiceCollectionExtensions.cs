using Microsoft.Extensions.DependencyInjection.Extensions;
using Stepping.Core.TransactionManagers;
using Stepping.TmProviders.LocalTm.DistributedLocks;
using Stepping.TmProviders.LocalTm.HostedService;
using Stepping.TmProviders.LocalTm.Steps;
using Stepping.TmProviders.LocalTm.Store;
using Stepping.TmProviders.LocalTm.TransactionManagers;

namespace Microsoft.Extensions.DependencyInjection;

public static class SteppingLocalTmServiceCollectionExtensions
{
    public static IServiceCollection AddSteppingLocalTm(this IServiceCollection services)
    {
        services.TryAddTransient<ILocalTmStepConverter, LocalTmStepConverter>();
        services.TryAddTransient<LocalTmStepConverter>();

        services.TryAddTransient<ILocalTmStepExecutor, LocalTmStepExecutor>();
        services.TryAddTransient<LocalTmStepExecutor>();

        services.TryAddTransient<ITmClient, LocalTmClient>();
        services.TryAddTransient<LocalTmClient>();

        services.TryAddTransient<ILocalTmManager, LocalTmManager>();
        services.TryAddTransient<LocalTmManager>();

        services.TryAddTransient<ISteppingDistributedLock, DefaultSteppingDistributedLock>();
        services.TryAddTransient<DefaultSteppingDistributedLock>();

        services.TryAddSingleton<MemoryLocalTmStore>();
        services.TryAddSingleton<ILocalTmStore>(sp => sp.GetRequiredService<MemoryLocalTmStore>());

        return services;
    }

    public static IServiceCollection AddLocalTmHostedService(this IServiceCollection services)
    {
        services.AddHostedService<LocalTmHostedService>();

        return services;
    }
}
