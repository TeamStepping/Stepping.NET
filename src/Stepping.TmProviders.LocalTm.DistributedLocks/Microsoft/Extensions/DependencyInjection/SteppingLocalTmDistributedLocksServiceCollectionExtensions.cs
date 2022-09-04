using Medallion.Threading;

namespace Microsoft.Extensions.DependencyInjection;

public static class SteppingLocalTmDistributedLocksServiceCollectionExtensions
{
    public static IServiceCollection AddLocalTmDistributedLockProvider(
        this IServiceCollection services, Func<IServiceProvider, IDistributedLockProvider> setupDistributedLockProvider)
    {
        services.AddSingleton(setupDistributedLockProvider);

        return services;
    }
}
