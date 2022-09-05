using Medallion.Threading;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Stepping.TmProviders.LocalTm.DistributedLocks;
using Stepping.TmProviders.LocalTm.Processors;

namespace Microsoft.Extensions.DependencyInjection;

public static class SteppingLocalTmHostedServiceProcessorServiceCollectionExtensions
{
    public static IServiceCollection AddLocalTmHostedServiceProcessor(this IServiceCollection services,
        Func<IServiceProvider, IDistributedLockProvider>? setupDistributedLockProvider = null)
    {
        if (setupDistributedLockProvider == null)
        {
            services.TryAddTransient<ISteppingDistributedLock, DefaultSteppingDistributedLock>();
            services.TryAddTransient<DefaultSteppingDistributedLock>();
        }
        else
        {
            services.AddSingleton(setupDistributedLockProvider);
            services.AddTransient<MedallionSteppingDistributedLock>();
            services.Replace(ServiceDescriptor.Singleton<ISteppingDistributedLock>(sp => sp.GetRequiredService<MedallionSteppingDistributedLock>()));
        }

        services.AddHostedService<HostedServiceLocalTmProcessor>();

        return services;
    }
}
