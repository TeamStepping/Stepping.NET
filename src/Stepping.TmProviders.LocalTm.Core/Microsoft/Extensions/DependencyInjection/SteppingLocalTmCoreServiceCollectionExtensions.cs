using Microsoft.Extensions.DependencyInjection.Extensions;
using Stepping.Core.TransactionManagers;
using Stepping.TmProviders.LocalTm;
using Stepping.TmProviders.LocalTm.Options;
using Stepping.TmProviders.LocalTm.Steps;
using Stepping.TmProviders.LocalTm.Store;
using Stepping.TmProviders.LocalTm.Timing;
using Stepping.TmProviders.LocalTm.TransactionManagers;

namespace Microsoft.Extensions.DependencyInjection;

public static class SteppingLocalTmCoreServiceCollectionExtensions
{
    public static IServiceCollection AddSteppingLocalTm(this IServiceCollection services)
    {
        return AddSteppingLocalTm(services, _ => { });
    }

    public static IServiceCollection AddSteppingLocalTm(this IServiceCollection services, Action<LocalTmOptions> actionOptions)
    {
        services.Configure<LocalTmOptions>(options => actionOptions?.Invoke(options));

        services.TryAddTransient<ILocalTmStepConverter, LocalTmStepConverter>();
        services.TryAddTransient<LocalTmStepConverter>();

        services.TryAddTransient<ITmClient, LocalTmClient>();
        services.TryAddTransient<LocalTmClient>();

        services.TryAddTransient<ILocalTmManager, LocalTmManager>();
        services.TryAddTransient<LocalTmManager>();

        services.TryAddTransient<MemoryTransactionStore>();
        services.TryAddTransient<ITransactionStore, MemoryTransactionStore>();

        services.TryAddSingleton<SteppingClock>();
        services.TryAddSingleton<ISteppingClock>(sp => sp.GetRequiredService<SteppingClock>());

        services.AddHttpClient(LocalTmConst.LocalTmHttpClient);

        return services;
    }
}
