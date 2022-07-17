using Microsoft.Extensions.DependencyInjection.Extensions;
using Stepping.Core.Databases;
using Stepping.Core.Infrastructures;
using Stepping.Core.Jobs;
using Stepping.Core.Options;
using Stepping.Core.Steps;

namespace Microsoft.Extensions.DependencyInjection;

public static class SteppingServiceCollectionExtensions
{
    public static IServiceCollection AddStepping(this IServiceCollection services, Action<SteppingOptions> setupAction)
    {
        services.AddSteppingServices();

        services.Configure<SteppingOptions>(options => { options.RegisterSteps(typeof(HttpRequestStep)); });

        services.Configure(setupAction);

        return services;
    }

    public static IServiceCollection AddStepping(this IServiceCollection services)
    {
        services.AddStepping(_ => { });

        return services;
    }

    private static IServiceCollection AddSteppingServices(this IServiceCollection services)
    {
        services.TryAddSingleton<IBarrierInfoModelFactory, BarrierInfoModelFactory>();
        services.TryAddSingleton<IStepNameProvider, StepNameProvider>();

        services.TryAddTransient<IDbBarrierInserterResolver, DbBarrierInserterResolver>();
        services.TryAddTransient<DbBarrierInserterResolver>();

        services.TryAddTransient<ISteppingDbContextProviderResolver, SteppingDbContextProviderResolver>();
        services.TryAddTransient<SteppingDbContextProviderResolver>();

        services.TryAddTransient<ISteppingTenantIdProvider, NullSteppingTenantIdProvider>();
        services.TryAddTransient<NullSteppingTenantIdProvider>();

        services.TryAddTransient<IConnectionStringHasher, Md5ConnectionStringHasher>();
        services.TryAddTransient<Md5ConnectionStringHasher>();

        services.TryAddTransient<ISteppingDbContextLookupInfoProvider, SteppingDbContextLookupInfoProvider>();
        services.TryAddTransient<SteppingDbContextLookupInfoProvider>();

        services.TryAddTransient<ISteppingJsonSerializer, NewtonsoftSteppingJsonSerializer>();
        services.TryAddTransient<NewtonsoftSteppingJsonSerializer>();

        services.TryAddTransient<IDistributedJobGidGenerator, DistributedJobGidGenerator>();
        services.TryAddTransient<DistributedJobGidGenerator>();

        services.TryAddTransient<IDistributedJobFactory, DistributedJobFactory>();
        services.TryAddTransient<DistributedJobFactory>();

        services.TryAddTransient<IStepArgsSerializer, JsonStepArgsSerializer>();
        services.TryAddTransient<JsonStepArgsSerializer>();

        services.TryAddTransient<IStepExecutor, StepExecutor>();
        services.TryAddTransient<StepExecutor>();

        services.TryAddTransient<IStepResolver, StepResolver>();
        services.TryAddTransient<StepResolver>();

        return services;
    }
}