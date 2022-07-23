using Microsoft.Extensions.DependencyInjection.Extensions;
using Stepping.Core.TransactionManagers;
using Stepping.TmProviders.LocalTm.DistributedLocks;
using Stepping.TmProviders.LocalTm.HostedService;
using Stepping.TmProviders.LocalTm.Steps;
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

        services.TryAddTransient<ILocalTmProcessor, LocalTmProcessor>();
        services.TryAddTransient<LocalTmProcessor>();

        services.AddHostedService<LocalTmHostedService>();

        services.TryAddTransient<ISteppingDistributedLock, DefaultSteppingDistributedLock>();
        services.TryAddTransient<DefaultSteppingDistributedLock>();

        return services;
    }
}
