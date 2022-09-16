using Microsoft.EntityFrameworkCore;
using Stepping.TmProviders.LocalTm.EfCore;
using Stepping.TmProviders.LocalTm.Store;

namespace Microsoft.Extensions.DependencyInjection;

public static class SteppingLocalTmEfCoreServiceCollectionExtensions
{
    public static IServiceCollection AddSteppingLocalTmEfCore(this IServiceCollection services, Action<DbContextOptionsBuilder> setupDbContextOptionsBuilder)
    {
        services.AddDbContext<LocalTmDbContext>(setupDbContextOptionsBuilder.Invoke);

        services.AddTransient<EfCoreTransactionStore>();
        services.AddTransient<ITransactionStore, EfCoreTransactionStore>();

        return services;
    }
}
