using Microsoft.EntityFrameworkCore;
using Stepping.TmProviders.LocalTm.EfCore;
using Stepping.TmProviders.LocalTm.Store;

namespace Microsoft.Extensions.DependencyInjection;

public static class SteppingLocalTmEfCoreServiceCollectionExtensions
{
    public static IServiceCollection AddSteppingLocalTmEfCore(this IServiceCollection services, Action<DbContextOptionsBuilder> setupDbContextOptionsBuilderA)
    {
        services.AddDbContext<LocalTmDbContext>(options => setupDbContextOptionsBuilderA.Invoke(options));

        services.AddTransient<EfCoreTransactionStore>();
        services.AddTransient<ITransactionStore, EfCoreTransactionStore>();

        return services;
    }
}
